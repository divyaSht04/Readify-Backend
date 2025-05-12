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

            // Update book stock
            var book = await _context.Books.FindAsync(cartItem.BookId);
            if (book != null)
            {
                book.StockQuantity -= cartItem.Quantity;
            }
        }

        // Save order
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Clear cart
        await _cartService.ClearCart(userId);

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