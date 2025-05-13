using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Context;
using Backend.Dtos;
using Backend.Model;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDBContext _context;
        private readonly IFileService _fileService;

        public BookService(ApplicationDBContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<ActionResult<List<BookResponse>>> GetAllBooks()
        {
            var now = DateTime.UtcNow;
            var books = await _context.Books.ToListAsync();
            
            // Get book-specific discounts for all books
            var bookResponses = new List<BookResponse>();
            foreach (var book in books)
            {
                var bookDiscount = await GetActiveBookDiscount(now, book.ID);
                bookResponses.Add(await MapToBookResponse(book, now, bookDiscount));
            }
            
            return bookResponses;
        }

        public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
        {
            var now = DateTime.UtcNow;
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return new NotFoundObjectResult($"Book with ID {id} not found.");
            }

            // Check for book-specific discount
            var discount = await GetActiveBookDiscount(now, id);
            return await MapToBookResponse(book, now, discount);
        }

        public async Task<ActionResult<BookResponse>> CreateBook(CreateBookRequest request)
        {
            string? imagePath = null;
            if (request.ImageFile != null)
            {
                imagePath = await _fileService.SaveFile(request.ImageFile, "books");
            }

            var book = new Book
            {
                ID = Guid.NewGuid(),
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN,
                Price = request.Price,
                Description = request.Description,
                PublishedDate = request.PublishedDate.HasValue
                    ? DateTime.SpecifyKind(request.PublishedDate.Value, DateTimeKind.Utc)
                    : null,
                StockQuantity = request.StockQuantity,
                IsComingSoon = request.IsComingSoon,
                ReleaseDate = request.ReleaseDate.HasValue
                    ? DateTime.SpecifyKind(request.ReleaseDate.Value, DateTimeKind.Utc)
                    : null,
                Category = request.Category,
                Image = imagePath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var bookDiscount = await GetActiveBookDiscount(now, book.ID);
            return await MapToBookResponse(book, now, bookDiscount);
        }

        public async Task<ActionResult<BookResponse>> UpdateBook(Guid id, UpdateBookRequest request)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return new NotFoundObjectResult($"Book with ID {id} not found.");
            }

            // Handle image upload
            if (request.ImageFile != null)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(book.Image))
                {
                    _fileService.DeleteFile(book.Image);
                }

                // Save new image
                string? imagePath = await _fileService.SaveFile(request.ImageFile, "books");
                book.Image = imagePath;
            }

            // Update only the properties that are provided
            if (request.Title != null)
                book.Title = request.Title;
            if (request.Author != null)
                book.Author = request.Author;
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
            if (request.IsComingSoon.HasValue)
                book.IsComingSoon = request.IsComingSoon.Value;
            if (request.ReleaseDate.HasValue)
                book.ReleaseDate = DateTime.SpecifyKind(request.ReleaseDate.Value, DateTimeKind.Utc);
            if (request.Category != null)
                book.Category = request.Category;

            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var bookDiscount = await GetActiveBookDiscount(now, book.ID);
            return await MapToBookResponse(book, now, bookDiscount);
        }

        public async Task<ActionResult> DeleteBook(Guid id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return new NotFoundObjectResult($"Book with ID {id} not found.");
            }

            // Delete associated image if it exists
            if (!string.IsNullOrEmpty(book.Image))
            {
                _fileService.DeleteFile(book.Image);
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
            var now = DateTime.UtcNow;

            var books = await _context.Books
                .Where(b => b.Title.ToLower().Contains(query) ||
                            b.Author.ToLower().Contains(query) ||
                            b.ISBN.ToLower().Contains(query) ||
                            b.Description.ToLower().Contains(query) ||
                            b.Category.Any(c => c.ToLower().Contains(query)))
                .ToListAsync();

            // Get book-specific discounts for each book
            var bookResponses = new List<BookResponse>();
            foreach (var book in books)
            {
                var bookDiscount = await GetActiveBookDiscount(now, book.ID);
                bookResponses.Add(await MapToBookResponse(book, now, bookDiscount));
            }

            return bookResponses;
        }

        public async Task<ActionResult<List<BookResponse>>> GetComingSoonBooks()
        {
            var now = DateTime.UtcNow;

            var books = await _context.Books
                .Where(b => b.IsComingSoon)
                .OrderBy(b => b.ReleaseDate)
                .ToListAsync();
            
            var bookResponses = new List<BookResponse>();
            foreach (var book in books)
            {
                var bookDiscount = await GetActiveBookDiscount(now, book.ID);
                bookResponses.Add(await MapToBookResponse(book, now, bookDiscount));
            }

            return bookResponses;
        }
        
        private async Task<Discount?> GetActiveBookDiscount(DateTime now, Guid bookId)
        {
            // Get book-specific discount
            return await _context.Discounts
                .Where(d => d.BookId == bookId && d.StartDate <= now && d.EndDate >= now)
                .OrderByDescending(d => d.CreatedAt) // Take the most recent discount if multiple overlap
                .FirstOrDefaultAsync();
        }
        
        public async Task<ActionResult<DiscountResponse>?> GetBookDiscount(Guid bookId)
        {
            var now = DateTime.UtcNow;
            var discount = await GetActiveBookDiscount(now, bookId);
            
            if (discount == null)
            {
                return null;
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
                CreatedAt = discount.CreatedAt,
                UpdatedAt = discount.UpdatedAt
            };
        }
        
        private async Task<BookResponse> MapToBookResponse(Book book, DateTime now, Discount? globalDiscount)
        {
            decimal? discountedPrice = null;
            decimal discountPercentage = 0;
            bool onSale = false;

            if (globalDiscount != null)
            {
                discountedPrice = book.Price - (book.Price * (globalDiscount.Percentage / 100));
                onSale = globalDiscount.OnSale;
                discountPercentage = globalDiscount.Percentage;
            }
            
            // Calculate book rating and review count
            var reviews = await _context.BookReviews
                .Where(r => r.BookId == book.ID)
                .ToListAsync();
            
            double? averageRating = null;
            if (reviews.Any())
            {
                averageRating = reviews.Average(r => r.Rating);
            }
            
            int reviewCount = reviews.Count;

            return new BookResponse
            {
                Id = book.ID,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Price = book.Price,
                DiscountedPrice = discountedPrice,
                OnSale = onSale,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                StockQuantity = book.StockQuantity,
                IsComingSoon = book.IsComingSoon,
                ReleaseDate = book.ReleaseDate,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                Category = book.Category,
                Image = book.Image,
                DiscountPercentage = discountPercentage,
                AverageRating = averageRating,
                ReviewCount = reviewCount
            };
        }
    }
}