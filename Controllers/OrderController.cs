using Backend.Dtos.Order;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/order")]
[Authorize]
public class OrderController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpPost("from-cart")]
    public async Task<ActionResult<OrderResponse>> CreateOrderFromCart([FromQuery] Guid userId)
    {
        return await _orderService.CreateOrderFromCart(userId);
    }
    
    [HttpGet("claim/{claimCode}")]
    public async Task<ActionResult<OrderResponse>> GetOrderByClaimCode(string claimCode)
    {
        return await _orderService.GetOrderByClaimCode(claimCode);
    }
    
    [HttpPost("verify/{claimCode}")]
    public async Task<ActionResult<OrderResponse>> VerifyOrderByClaimCode(string claimCode)
    {
        return await _orderService.VerifyOrderByClaimCode(claimCode);
    }
}