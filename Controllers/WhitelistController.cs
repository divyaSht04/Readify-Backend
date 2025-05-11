using Backend.Context;
using Backend.Dtos;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("/whitelist")]
[Authorize]
public class WhitelistController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public WhitelistController(ApplicationDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WhitelistResponseDto>>> GetUserWhitelist([FromQuery] Guid userId)
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

        return Ok(whitelist);
    }

    [HttpPost]
    public async Task<ActionResult<WhitelistResponseDto>> AddToWhitelist([FromQuery] Guid userId, [FromForm] AddToWhitelistDto dto)
    {
        // Check if book exists
        var book = await _context.Books.FindAsync(dto.BookId);
        if (book == null)
            return NotFound("Book not found");

        // Check if already in whitelist
        var existingWhitelist = await _context.Whitelists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == dto.BookId);
        
        if (existingWhitelist != null)
            return BadRequest("Book is already in whitelist");

        var whitelist = new Whitelist
        {
            UserId = userId,
            BookId = dto.BookId
        };

        _context.Whitelists.Add(whitelist);
        await _context.SaveChangesAsync();

        return Ok(new WhitelistResponseDto
        {
            Id = whitelist.Id,
            BookId = book.ID,
            BookTitle = book.Title,
            BookAuthor = book.Author,
            BookImage = book.Image,
            CreatedAt = whitelist.CreatedAt
        });
    }

    [HttpDelete("{bookId}")]
    public async Task<IActionResult> RemoveFromWhitelist(Guid bookId, [FromQuery] Guid userId)
    {
        var whitelist = await _context.Whitelists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
        
        if (whitelist == null)
            return NotFound("Book not found in whitelist");

        _context.Whitelists.Remove(whitelist);
        await _context.SaveChangesAsync();

        return NoContent();
    }
} 