using Backend.Context;
using Backend.Dtos;
using Backend.Model;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DiscountService: IDiscountService
{
    private readonly ApplicationDBContext _context;
    
    public DiscountService(ApplicationDBContext context)
    {
        _context = context;
    }
    
    public async Task<ActionResult<DiscountResponse>> SetBookDiscount(Guid bookId, CreateDiscountRequest request)
    {
            if (bookId.ToString() == null || bookId.ToString() == "")
            {
                return new BadRequestObjectResult("BookId is required for setting a book discount.");
            }
            
            var startUtc = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return new NotFoundObjectResult($"Book with ID {bookId} not found.");
            }
            string bookTitle = book.Title;
            
            var existingBookDiscounts = await _context.Discounts
                .Where(d => d.BookId == bookId && d.StartDate <= endUtc && d.EndDate >= startUtc)
                .ToListAsync();
                
            if (existingBookDiscounts.Any())
            {
                _context.Discounts.RemoveRange(existingBookDiscounts);
            }

            var discount = new Discount
            {
                Id = Guid.NewGuid(),
                DiscountName = request.DiscountName,
                Percentage = request.Percentage,
                StartDate = startUtc,
                EndDate = endUtc,
                OnSale = request.OnSale,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return new DiscountResponse
            {
                Id = discount.Id,
                DiscountName = discount.DiscountName,
                Percentage = discount.Percentage,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                OnSale = discount.OnSale,
                BookId = discount.BookId,
                BookTitle = bookTitle,
                CreatedAt = discount.CreatedAt,
                UpdatedAt = discount.UpdatedAt
            };
        }
        
        public async Task<ActionResult> RemoveBookDiscount(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return new NotFoundObjectResult($"Book with ID {bookId} not found.");
            }
            
            var activeDiscounts = await _context.Discounts
                .Where(d => d.BookId == bookId && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            if (!activeDiscounts.Any())
            {
                return new NotFoundObjectResult($"No active discount found for book with ID {bookId}.");
            }

            _context.Discounts.RemoveRange(activeDiscounts);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = $"Discount for book '{book.Title}' removed successfully" });
        }
}
