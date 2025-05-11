using Backend.Dtos.Cart;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface ICartService
{
    Task<ActionResult<CartResponse>> GetCart(Guid userId);
    Task<ActionResult<CartResponse>> AddItemToCart(Guid userId, AddCartItemRequest request);
    Task<ActionResult<CartResponse>> UpdateCartItem(Guid userId, Guid cartId, Guid bookId, UpdateCartItemRequest request);
    Task<ActionResult<CartResponse>> RemoveCartItem(Guid userId, Guid cartId, Guid bookId);
    Task<ActionResult> ClearCart(Guid userId);
} 