using Backend.Context;
using Backend.Dtos.BookAccolade;
using Backend.enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BookAccoladeService : IBookAccoladeService
{
    private readonly ApplicationDBContext _context;

    public BookAccoladeService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookAccoladeResponse>> GetAllAccolades()
    {
        return await _context.BookAccolades
            .Include(a => a.Book)
            .Select(a => MapToResponse(a))
            .ToListAsync();
    }

    public async Task<IEnumerable<BookAccoladeResponse>> GetAccoladesByType(AccoladeType type)
    {
        return await _context.BookAccolades
            .Include(a => a.Book)
            .Where(a => a.AccoladeType == type)
            .Select(a => MapToResponse(a))
            .ToListAsync();
    }

    public async Task<IEnumerable<BookAccoladeResponse>> GetAccoladesByBookId(Guid bookId)
    {
        return await _context.BookAccolades
            .Include(a => a.Book)
            .Where(a => a.BookID == bookId)
            .Select(a => MapToResponse(a))
            .ToListAsync();
    }

    public async Task<BookAccoladeResponse> GetAccoladeById(Guid id)
    {
        var accolade = await _context.BookAccolades
            .Include(a => a.Book)
            .FirstOrDefaultAsync(a => a.ID == id);

        if (accolade == null)
        {
            throw new KeyNotFoundException($"Accolade with ID {id} not found");
        }

        return MapToResponse(accolade);
    }

    public async Task<BookAccoladeResponse> CreateAccolade(CreateBookAccoladeRequest request)
    {
        var book = await _context.Books.FindAsync(request.BookID);
        if (book == null)
        {
            throw new KeyNotFoundException($"Book with ID {request.BookID} not found");
        }

        var accolade = new BookAccolade
        {
            ID = Guid.NewGuid(),
            AccoladeType = request.AccoladeType,
            Name = request.Name,
            Description = request.Description,
            AwardedDate = request.AwardedDate,
            Category = request.Category,
            BookID = request.BookID,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.BookAccolades.AddAsync(accolade);
        await _context.SaveChangesAsync();

        // Reload the accolade with book information
        accolade = await _context.BookAccolades
            .Include(a => a.Book)
            .FirstOrDefaultAsync(a => a.ID == accolade.ID);

        return MapToResponse(accolade);
    }

    public async Task<BookAccoladeResponse> UpdateAccolade(Guid id, UpdateBookAccoladeRequest request)
    {
        var accolade = await _context.BookAccolades
            .Include(a => a.Book)
            .FirstOrDefaultAsync(a => a.ID == id);

        if (accolade == null)
        {
            throw new KeyNotFoundException($"Accolade with ID {id} not found");
        }

        accolade.AccoladeType = request.AccoladeType;
        accolade.Name = request.Name;
        accolade.Description = request.Description;
        accolade.AwardedDate = request.AwardedDate;
        accolade.Category = request.Category;
        accolade.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(accolade);
    }

    public async Task<bool> DeleteAccolade(Guid id)
    {
        var accolade = await _context.BookAccolades.FindAsync(id);
        if (accolade == null)
        {
            return false;
        }

        _context.BookAccolades.Remove(accolade);
        await _context.SaveChangesAsync();
        return true;
    }

    private static BookAccoladeResponse MapToResponse(BookAccolade accolade)
    {
        return new BookAccoladeResponse
        {
            ID = accolade.ID,
            AccoladeType = accolade.AccoladeType,
            Name = accolade.Name,
            Description = accolade.Description,
            AwardedDate = accolade.AwardedDate,
            Category = accolade.Category,
            BookID = accolade.BookID,
            BookTitle = accolade.Book?.Title,
            Author = accolade.Book?.Author,
            CreatedAt = accolade.CreatedAt,
            UpdatedAt = accolade.UpdatedAt
        };
    }
}
