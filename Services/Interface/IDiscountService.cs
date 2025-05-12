using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IDiscountService
{
    Task<ActionResult<DiscountResponse>> SetBookDiscount(Guid bookId, CreateDiscountRequest request);
    Task<ActionResult> RemoveBookDiscount(Guid bookId);
}