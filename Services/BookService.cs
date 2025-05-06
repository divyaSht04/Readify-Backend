using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Context;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BookService : IBookService
{
    private readonly ApplicationDBContext _context;

    public BookService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<List<BookResponse>>> GetAllBooks()
    {
        var books = await _context.Books.ToListAsync();
        return books.Select(MapToBookResponse).ToList();
    }

    public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {id} not found.");
        }
        
        return MapToBookResponse(book);
    }

    public async Task<ActionResult<BookResponse>> CreateBook(CreateBookRequest request)
    {
        var book = new Book
        {
            ID = Guid.NewGuid(),
            Title = request.Title,
            ISBN = request.ISBN,
            Price = request.Price,
            Description = request.Description,
            PublishedDate = request.PublishedDate.HasValue ? DateTime.SpecifyKind(request.PublishedDate.Value, DateTimeKind.Utc) : null,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        
        return MapToBookResponse(book);
    }

    public async Task<ActionResult<BookResponse>> UpdateBook(Guid id, UpdateBookRequest request)
    {
        var book = await _context.Books.FindAsync(id);
        
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {id} not found.");
        }
        
        // Update only the properties that are provided
        if (request.Title != null)
            book.Title = request.Title;
            
        if (request.ISBN != null)
            book.ISBN = request.ISBN;
            
        if (request.Price.HasValue)
            book.Price = request.Price.Value;
            
        if (request.Description != null)
            book.Description = request.Description;
            
        if (request.PublishedDate.HasValue)
            book.PublishedDate = DateTime.SpecifyKind(request.PublishedDate.Value, DateTimeKind.Utc);
            
        if (request.StockQuantity.HasValue)
            book.StockQuantity = request.StockQuantity.Value;
            
        if (request.Category != null)
            book.Category = request.Category;
            
        book.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return MapToBookResponse(book);
    }

    public async Task<ActionResult> DeleteBook(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {id} not found.");
        }
        
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        
        return new OkResult();
    }

    public async Task<ActionResult<List<BookResponse>>> SearchBooks(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllBooks();
        }
        
        query = query.ToLower();
        
        var books = await _context.Books
            .Where(b => b.Title.ToLower().Contains(query) || 
                   b.ISBN.ToLower().Contains(query) ||
                   b.Description.ToLower().Contains(query) ||
                   b.Category.Any(c => c.ToLower().Contains(query)))
            .ToListAsync();
            
        return books.Select(MapToBookResponse).ToList();
    }
    
    // Helper method to map from Book entity to BookResponse DTO
    private static BookResponse MapToBookResponse(Book book)
    {
        return new BookResponse
        {
            Id = book.ID,
            Title = book.Title,
            ISBN = book.ISBN,
            Price = book.Price,
            Description = book.Description,
            PublishedDate = book.PublishedDate,
            StockQuantity = book.StockQuantity,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Category = book.Category
        };
    }
}
