using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IBookService
{
    Task<ActionResult<List<BookResponse>>> GetAllBooks();
    Task<ActionResult<BookResponse>> GetBookById(Guid id);
    Task<ActionResult<BookResponse>> CreateBook(CreateBookRequest request);
    Task<ActionResult<BookResponse>> UpdateBook(Guid id, UpdateBookRequest request);
    Task<ActionResult> DeleteBook(Guid id);
    Task<ActionResult<List<BookResponse>>> SearchBooks(string query);
    Task<ActionResult<List<BookResponse>>> GetComingSoonBooks();
    Task<ActionResult<DiscountResponse>?> GetBookDiscount(Guid bookId);
    Task<ActionResult<List<BookResponse>>> GetBestSellerBooks(int limit = 10);
}
