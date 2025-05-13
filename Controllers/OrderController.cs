using Backend.Dtos.Order;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/order")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("from-cart")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> CreateOrderFromCart([FromQuery] Guid userId)
    {
        return await _orderService.CreateOrderFromCart(userId);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<OrderResponse>>> GetUserOrders(Guid userId)
    {
        return await _orderService.GetUserOrders(userId);
    }

    [HttpGet("claim/{claimCode}")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> GetOrderByClaimCode(string claimCode)
    {
        return await _orderService.GetOrderByClaimCode(claimCode);
    }

    [HttpPost("verify/{claimCode}")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> VerifyOrderByClaimCode(string claimCode)
    {
        return await _orderService.VerifyOrderByClaimCode(claimCode);
    }
}