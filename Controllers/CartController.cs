using Backend.Dtos.Cart;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart([FromQuery] Guid userId)
    {
        return await _cartService.GetCart(userId);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItemToCart([FromQuery] Guid userId, AddCartItemRequest request)
    {
        return await _cartService.AddItemToCart(userId, request);
    }

    [HttpPut("{cartId}/items/{bookId}")]
    public async Task<ActionResult<CartResponse>> UpdateCartItem(Guid cartId, Guid bookId, [FromQuery] Guid userId, UpdateCartItemRequest request)
    {
        return await _cartService.UpdateCartItem(userId, cartId, bookId, request);
    }

    [HttpDelete("{cartId}/items/{bookId}")]
    public async Task<ActionResult<CartResponse>> RemoveCartItem(Guid cartId, Guid bookId, [FromQuery] Guid userId)
    {
        return await _cartService.RemoveCartItem(userId, cartId, bookId);
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart([FromQuery] Guid userId)
    {
        return await _cartService.ClearCart(userId);
    }
} 