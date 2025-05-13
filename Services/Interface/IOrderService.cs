using Backend.Dtos.Order;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IOrderService
{
    Task<ActionResult<OrderResponse>> CreateOrderFromCart(Guid userId);
    Task<ActionResult<List<OrderResponse>>> GetUserOrders(Guid userId);
    Task<ActionResult<OrderResponse>> GetOrderByClaimCode(string claimCode);
    Task<ActionResult<OrderResponse>> VerifyOrderByClaimCode(string claimCode);
}