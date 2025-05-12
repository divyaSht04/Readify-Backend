using Backend.Dtos.Order;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public class OrderService : IOrderService
{
    public Task<ActionResult<OrderResponse>> CreateOrderFromCart(Guid userId)
    {
        throw new NotImplementedException();
    }
}