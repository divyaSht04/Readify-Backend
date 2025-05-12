using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/discount")]
[Authorize(Roles = "ADMIN")]
public class DiscountController
{
    private readonly DiscountService _discountService;

    public DiscountController(DiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpPost("/book/{bookId}")]
    public async Task<ActionResult<DiscountResponse>> SetBookDiscount(Guid bookId, [FromBody] CreateDiscountRequest request)
    {
        return await _discountService.SetBookDiscount(bookId, request);
    }

    [HttpDelete("/book/{bookId}")]
    public async Task<ActionResult> RemoveBookDiscount(Guid bookId)
    {
        return await _discountService.RemoveBookDiscount(bookId);
    }
}