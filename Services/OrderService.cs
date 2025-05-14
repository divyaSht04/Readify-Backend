using Backend.Context;
using Backend.Dtos.Order;
using Backend.Model;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDBContext _context;
    private readonly ICartService _cartService;
    private readonly IEmailService _emailService;

    public OrderService(ApplicationDBContext context, ICartService cartService, IEmailService emailService)
    {
        _context = context;
        _cartService = cartService;
        _emailService = emailService;
    }

    public async Task<ActionResult<OrderResponse>> CreateOrderFromCart(Guid userId)
    {
        var cartResult = await _cartService.GetCart(userId);
        if (cartResult.Value == null)
        {
            return new BadRequestObjectResult("Failed to retrieve cart");
        }
        
        var cartResponse = cartResult.Value;
        if (!cartResponse.Items.Any())
        {
            return new BadRequestObjectResult("Cart is empty");
        }
        
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new NotFoundObjectResult("User not found");
        }
        
        var claimCode = ClaimCodeUtil.GenerateClaimCode();
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClaimCode = claimCode,
            TotalAmount = cartResponse.TotalPrice,
            OriginalTotalAmount = cartResponse.OriginalTotalPrice,
            VolumeDiscountAmount = cartResponse.VolumeDiscountAmount,
            LoyaltyDiscountAmount = cartResponse.LoyaltyDiscountAmount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        
        foreach (var cartItem in cartResponse.Items)
        {
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                BookId = cartItem.BookId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.BookPrice,
                DiscountedPrice = cartItem.DiscountedPrice,
                DiscountPercentage = cartItem.DiscountPercentage,
                TotalPrice = cartItem.TotalPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            order.Items.Add(orderItem);

            // NOTE: We've removed the inventory reduction here
            // It will now happen at verification time
        }

        // Save order
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // NOTE: We've removed the cart clearing here
        // It will now happen at verification time

        // Send confirmation email
        await _emailService.SendOrderConfirmationEmailAsync(
            user.Email,
            claimCode,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookAuthor = i.Book.Author,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountedPrice = i.DiscountedPrice,
                DiscountPercentage = i.DiscountPercentage,
                TotalPrice = i.TotalPrice,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList()
        );

        return await MapToOrderResponse(order);
    }
    
    public async Task<ActionResult<OrderResponse>> GetOrderByClaimCode(string claimCode)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(o => o.ClaimCode == claimCode);

        if (order == null)
        {
            return new NotFoundObjectResult($"Order with claim code {claimCode} not found");
        }

        return await MapToOrderResponse(order);
    }
    
    public async Task<ActionResult<OrderResponse>> VerifyOrderByClaimCode(string claimCode)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.ClaimCode == claimCode);

        if (order == null)
        {
            return new NotFoundObjectResult($"Order with claim code {claimCode} not found");
        }
        
        if (order.Status == "Verified" || order.Status == "Completed")
        {
            return new BadRequestObjectResult($"Order with claim code {claimCode} has already been verified");
        }
        
        order.Status = "Verified";
        order.UpdatedAt = DateTime.UtcNow;
        
        foreach (var orderItem in order.Items)
        {
            var book = await _context.Books.FindAsync(orderItem.BookId);
            if (book != null)
            {
                if (book.StockQuantity < orderItem.Quantity)
                {
                    return new BadRequestObjectResult($"Not enough stock available for book: {book.Title}");
                }
                
                book.StockQuantity -= orderItem.Quantity;
            }
            else
            {
                return new NotFoundObjectResult($"Book with ID {orderItem.BookId} not found");
            }
        }
        
        await _context.SaveChangesAsync();
        await _cartService.ClearCart(order.UserId);
        
        if (order.User != null && !string.IsNullOrEmpty(order.User.Email))
        {
            await _emailService.SendOrderVerificationEmailAsync(
                order.User.Email,
                order.ClaimCode,
                order.TotalAmount,
                order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    BookId = i.BookId,
                    BookTitle = i.Book.Title,
                    BookAuthor = i.Book.Author,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountedPrice = i.DiscountedPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    TotalPrice = i.TotalPrice,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList()
            );
        }
        
        return await MapToOrderResponse(order);
    }
    
    public async Task<ActionResult<List<OrderResponse>>> GetUserOrders(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        if (orders == null || !orders.Any())
        {
            return new List<OrderResponse>();
        }

        var orderResponses = new List<OrderResponse>();
        foreach (var order in orders)
        {
            orderResponses.Add(await MapToOrderResponse(order));
        }

        return orderResponses;
    }
    
    private async Task<OrderResponse> MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            ClaimCode = order.ClaimCode,
            TotalAmount = order.TotalAmount,
            OriginalTotalAmount = order.OriginalTotalAmount,
            VolumeDiscountAmount = order.VolumeDiscountAmount,
            LoyaltyDiscountAmount = order.LoyaltyDiscountAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookAuthor = i.Book.Author,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountedPrice = i.DiscountedPrice,
                DiscountPercentage = i.DiscountPercentage,
                TotalPrice = i.TotalPrice,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList()
        };
    }
}