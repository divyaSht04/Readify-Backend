using Backend.Context;
using Backend.Dtos;
using Backend.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class WhiteListService: IWhiteListService
{
    private readonly ApplicationDBContext _context;

    public WhiteListService(ApplicationDBContext context)
    {
        _context = context;
    }
    
    public async Task<ActionResult<BookResponse>> AddWhiteListAsync(Guid bookId, Guid userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return new NotFoundObjectResult("Book not found");

        // Check if already in whitelist
        var existingWhitelist = await _context.Whitelists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
        
        if (existingWhitelist != null)
            return new BadRequestObjectResult("Book is already in whitelist");

        var whitelist = new Whitelist
        {
            UserId = userId,
            BookId = bookId
        };

        _context.Whitelists.Add(whitelist);
        await _context.SaveChangesAsync();

        return new OkObjectResult(new BookResponse
        {
            Id = book.ID,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Price = book.Price,
            DiscountedPrice = book.DiscountedPrice,
            OnSale = book.OnSale,
            Description = book.Description,
            PublishedDate = book.PublishedDate,
            StockQuantity = book.StockQuantity,
            IsComingSoon = book.IsComingSoon,
            ReleaseDate = book.ReleaseDate,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Category = book.Category,
            Image = book.Image
        });
    }

    public async Task<ActionResult<BookResponse>> RemoveFromWhitelist(Guid bookId, Guid userId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return new NotFoundObjectResult("Book not found");

        var existingWhitelist = await _context.Whitelists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);

        if (existingWhitelist == null)
            return new BadRequestObjectResult("Book is not in the whitelist");

        _context.Whitelists.Remove(existingWhitelist);
        await _context.SaveChangesAsync();

        return new OkObjectResult(new BookResponse
        {
            Id = book.ID,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Price = book.Price,
            DiscountedPrice = book.DiscountedPrice,
            OnSale = book.OnSale,
            Description = book.Description,
            PublishedDate = book.PublishedDate,
            StockQuantity = book.StockQuantity,
            IsComingSoon = book.IsComingSoon,
            ReleaseDate = book.ReleaseDate,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Category = book.Category,
            Image = book.Image
        });
    }

    public async Task<ActionResult<List<BookResponse>>> GetUserWhitelist(Guid userId)
    {
        var whitelist = await _context.Whitelists
            .Include(w => w.Book)
            .Where(w => w.UserId == userId)
            .Select(w => new BookResponse
            {
                Id = w.Book!.ID,
                Title = w.Book.Title,
                Author = w.Book.Author,
                ISBN = w.Book.ISBN,
                Price = w.Book.Price,
                DiscountedPrice = w.Book.DiscountedPrice,
                OnSale = w.Book.OnSale,
                Description = w.Book.Description,
                PublishedDate = w.Book.PublishedDate,
                StockQuantity = w.Book.StockQuantity,
                IsComingSoon = w.Book.IsComingSoon,
                ReleaseDate = w.Book.ReleaseDate,
                CreatedAt = w.Book.CreatedAt,
                UpdatedAt = w.Book.UpdatedAt,
                Category = w.Book.Category,
                Image = w.Book.Image
            })
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return new OkObjectResult(whitelist);
    }
}