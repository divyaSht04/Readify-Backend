using Backend.Context;
using Backend.Dtos;
using Backend.Dtos.Cart;
using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Services;

public class CartService : ICartService
{
    private readonly ApplicationDBContext _context;
    private readonly IBookService _bookService;
    private readonly ILoyaltyDiscountService _loyaltyDiscountService;

    public CartService(ApplicationDBContext context, IBookService bookService, ILoyaltyDiscountService loyaltyDiscountService)
    {
        _context = context;
        _bookService = bookService;
        _loyaltyDiscountService = loyaltyDiscountService;
    }

    public async Task<ActionResult<CartResponse>> GetCart(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        return await MapToCartResponse(cart);
    }

    public async Task<ActionResult<CartResponse>> AddItemToCart(Guid userId, AddCartItemRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.Carts.AddAsync(cart);
        }

        var book = await _context.Books.FindAsync(request.BookId);
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {request.BookId} not found.");
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.BookId == request.BookId);
        if (existingItem != null)
        {
            return await MapToCartResponse(cart);
        }
        else
        {
            var cartItem = new CartItem
            {
                BookId = request.BookId,
                CartId = cart.Id,
                Quantity = 1, // Always 1 when adding
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            cart.Items.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToCartResponse(cart);
    }

    public async Task<ActionResult<CartResponse>> UpdateCartItem(Guid userId, Guid cartId, Guid bookId, UpdateCartItemRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cartId);

        if (cart == null)
        {
            return new NotFoundObjectResult($"Cart not found for user {userId} and cart {cartId}");
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.BookId == bookId && i.CartId == cartId);
        if (cartItem == null)
        {
            return new NotFoundObjectResult($"Cart item with BookId {bookId} not found in cart {cartId}");
        }

        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {bookId} not found.");
        }

        if (request.Quantity > book.StockQuantity)
        {
            return new BadRequestObjectResult($"Not enough stock available. Current stock: {book.StockQuantity}");
        }

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToCartResponse(cart);
    }

    public async Task<ActionResult<CartResponse>> RemoveCartItem(Guid userId, Guid cartId, Guid bookId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cartId);

        if (cart == null)
        {
            return new NotFoundObjectResult($"Cart not found for user {userId} and cart {cartId}");
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.BookId == bookId && i.CartId == cartId);
        if (cartItem == null)
        {
            return new NotFoundObjectResult($"Cart item with BookId {bookId} not found in cart {cartId}");
        }

        cart.Items.Remove(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToCartResponse(cart);
    }

    public async Task<ActionResult> ClearCart(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return new NotFoundObjectResult($"Cart not found for user {userId}");
        }

        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkResult();
    }

    private async Task<CartResponse> MapToCartResponse(Cart cart)
    {
        var now = DateTime.UtcNow;
        var cartResponse = new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = new List<CartItemResponse>(),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            HasVolumeDiscount = false,
            VolumeDiscountMessage = string.Empty,
            HasLoyaltyDiscount = false,
            LoyaltyDiscountMessage = string.Empty,
            LoyaltyDiscountAmount = 0
        };
        
        decimal totalPrice = 0;
        int totalQuantity = 0;
        
        foreach (var item in cart.Items)
        {
            var discount = await _bookService.GetBookDiscount(item.BookId);
            
            decimal effectivePrice = item.Book.Price;
            decimal? discountedPrice = null;
            decimal? discountPercentage = null;
            bool onSale = false;
            
            if (discount != null && discount.Value is DiscountResponse discountInfo)
            {
                discountedPrice = item.Book.Price - (item.Book.Price * (discountInfo.Percentage / 100));
                discountPercentage = discountInfo.Percentage;
                onSale = discountInfo.OnSale;
                effectivePrice = discountedPrice.Value;
            }
            
            // Calculate total price for this item (using discounted price if available)
            decimal itemTotalPrice = effectivePrice * item.Quantity;
            totalPrice += itemTotalPrice;
            totalQuantity += item.Quantity;
            
            cartResponse.Items.Add(new CartItemResponse
            {
                BookId = item.BookId,
                BookTitle = item.Book.Title,
                BookAuthor = item.Book.Author,
                BookPrice = item.Book.Price,
                DiscountedPrice = discountedPrice,
                DiscountPercentage = discountPercentage,
                OnSale = onSale,
                Quantity = item.Quantity,
                TotalPrice = itemTotalPrice,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            });
        }
        
        // Store the original total price before any discounts
        cartResponse.OriginalTotalPrice = totalPrice;
        
        // Apply 5% volume discount if total quantity is 5 or more
        if (totalQuantity >= 5)
        {
            cartResponse.HasVolumeDiscount = true;
            cartResponse.VolumeDiscountAmount = Math.Round(totalPrice * 0.05m, 2);
            totalPrice = totalPrice - cartResponse.VolumeDiscountAmount;
            cartResponse.VolumeDiscountMessage = $"5% discount applied for ordering {totalQuantity} books!";
        }
        
        // Apply 10% loyalty discount if user has exactly 10 successful orders
        bool qualifiesForLoyaltyDiscount = await _loyaltyDiscountService.QualifiesForLoyaltyDiscount(cart.UserId);
        if (qualifiesForLoyaltyDiscount)
        {
            decimal loyaltyDiscountPercentage = await _loyaltyDiscountService.GetLoyaltyDiscountPercentage(cart.UserId);
            cartResponse.HasLoyaltyDiscount = true;
            cartResponse.LoyaltyDiscountAmount = Math.Round(totalPrice * (loyaltyDiscountPercentage / 100m), 2);
            totalPrice = totalPrice - cartResponse.LoyaltyDiscountAmount;
            cartResponse.LoyaltyDiscountMessage = $"{loyaltyDiscountPercentage}% loyalty discount applied for your 10th order!";
        }
        else 
        {
            // Get current count to inform user about progress
            int currentOrderCount = await _loyaltyDiscountService.GetCompletedOrdersCount(cart.UserId);
            if (currentOrderCount > 0)
            {
                cartResponse.LoyaltyDiscountMessage = $"You've completed {currentOrderCount} order(s). Get a 10% discount on your 10th order!";
            }
        }
        
        cartResponse.TotalPrice = totalPrice;
        return cartResponse;
    }
} 