using Backend.Context;
using Backend.Dtos.Cart;
using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CartService : ICartService
{
    private readonly ApplicationDBContext _context;

    public CartService(ApplicationDBContext context)
    {
        _context = context;
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

        return MapToCartResponse(cart);
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
            return MapToCartResponse(cart);
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

        return MapToCartResponse(cart);
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

        return MapToCartResponse(cart);
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

        return MapToCartResponse(cart);
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

    private static CartResponse MapToCartResponse(Cart cart)
    {
        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(item => new CartItemResponse
            {
                BookId = item.BookId,
                BookTitle = item.Book.Title,
                BookAuthor = item.Book.Author,
                BookPrice = item.Book.Price,
                Quantity = item.Quantity,
                TotalPrice = item.Book.Price * item.Quantity,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            }).ToList(),
            TotalPrice = cart.Items.Sum(item => item.Book.Price * item.Quantity),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
} 