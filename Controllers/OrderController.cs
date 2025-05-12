using Backend.Dtos.Order;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

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
    
}