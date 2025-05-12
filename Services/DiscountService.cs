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

    public async Task<ActionResult<List<DiscountResponse>>> GetAllDiscounts()
    {
        var discounts = await _context.Discounts
            .Include(d => d.Book)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return discounts.Select(d => new DiscountResponse
        {
            Id = d.Id,
            DiscountName = d.DiscountName,
            Percentage = d.Percentage,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            OnSale = d.OnSale,
            BookId = d.BookId,
            BookTitle = d.Book?.Title,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();
    }

    public async Task<ActionResult<DiscountResponse>> GetDiscountById(Guid id)
    {
        var discount = await _context.Discounts
            .Include(d => d.Book)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (discount == null)
        {
            return new NotFoundObjectResult($"Discount with ID {id} not found.");
        }

        return new DiscountResponse
        {
            Id = discount.Id,
            DiscountName = discount.DiscountName,
            Percentage = discount.Percentage,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            OnSale = discount.OnSale,
            BookId = discount.BookId,
            BookTitle = discount.Book?.Title,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
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

        // Update the book's discounted price and on sale status
        if (book != null)
        {
            book.DiscountedPrice = book.Price * (1 - (request.Percentage / 100m));
            book.OnSale = request.OnSale;
            await _context.SaveChangesAsync();
        }

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

    public async Task<ActionResult> DeleteDiscount(Guid id)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if (discount == null)
        {
            return new NotFoundObjectResult($"Discount with ID {id} not found.");
        }

        _context.Discounts.Remove(discount);
        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Discount deleted successfully" });
    }

    public async Task<ActionResult<DiscountResponse>> UpdateDiscount(Guid id, CreateDiscountRequest request)
    {
        var discount = await _context.Discounts.FindAsync(id);
        if (discount == null)
        {
            return new NotFoundObjectResult($"Discount with ID {id} not found.");
        }

        var startUtc = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(request.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        discount.DiscountName = request.DiscountName;
        discount.Percentage = request.Percentage;
        discount.StartDate = startUtc;
        discount.EndDate = endUtc;
        discount.OnSale = request.OnSale;
        discount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var book = await _context.Books.FindAsync(discount.BookId);
        return new DiscountResponse
        {
            Id = discount.Id,
            DiscountName = discount.DiscountName,
            Percentage = discount.Percentage,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            OnSale = discount.OnSale,
            BookId = discount.BookId,
            BookTitle = book?.Title,
            CreatedAt = discount.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }
}
