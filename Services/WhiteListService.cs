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
    
    
    public async Task<ActionResult<WhitelistResponseDto>> AddWhiteListAsync(Guid bookId, Guid userId)
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

        return new OkObjectResult(new WhitelistResponseDto
        {
            Id = whitelist.Id,
            BookId = book.ID,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            BookImage = book.Image,
            CreatedAt = whitelist.CreatedAt
        });
    }

    public async Task<ActionResult<WhitelistResponseDto>> RemoveFromWhitelist(Guid bookId, Guid userId)
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

        var response = new WhitelistResponseDto
        {
            Id = existingWhitelist.Id,
            BookId = book.ID,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            BookImage = book.Image,
            CreatedAt = existingWhitelist.CreatedAt
        };

        return new OkObjectResult(response);
    }



    public async Task<ActionResult<WhitelistResponseDto>> GetUserWhitelist(Guid userId)
    {
        var whitelist = await _context.Whitelists
            .Include(w => w.Book)
            .Where(w => w.UserId == userId)
            .Select(w => new WhitelistResponseDto
            {
                Id = w.Id,
                BookId = w.BookId,
                BookTitle = w.Book!.Title,
                BookAuthor = w.Book.Author,
                BookImage = w.Book.Image,
                CreatedAt = w.CreatedAt
            })
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return new OkObjectResult(whitelist);
    }
}