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
    private readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DiscountResponse>>> GetAllDiscounts()
    {
        return await _discountService.GetAllDiscounts();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DiscountResponse>> GetDiscountById(Guid id)
    {
        return await _discountService.GetDiscountById(id);
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDiscount(Guid id)
    {
        return await _discountService.DeleteDiscount(id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DiscountResponse>> UpdateDiscount(Guid id, [FromBody] CreateDiscountRequest request)
    {
        return await _discountService.UpdateDiscount(id, request);
    }
}