using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IDiscountService
{
    Task<ActionResult<List<DiscountResponse>>> GetAllDiscounts();
    Task<ActionResult<DiscountResponse>> GetDiscountById(Guid id);
    Task<ActionResult<DiscountResponse>> SetBookDiscount(Guid bookId, CreateDiscountRequest request);
    Task<ActionResult> RemoveBookDiscount(Guid bookId);
    Task<ActionResult> DeleteDiscount(Guid id);
    Task<ActionResult<DiscountResponse>> UpdateDiscount(Guid id, CreateDiscountRequest request);
}