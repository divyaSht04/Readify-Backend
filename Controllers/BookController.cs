using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("/books")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    
    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<BookResponse>>> GetAllBooks()
    {
        return await _bookService.GetAllBooks();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
    {
        return await _bookService.GetBookById(id);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookResponse>> CreateBook([FromForm] CreateBookRequest request)
    {
        return await _bookService.CreateBook(request);
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<BookResponse>> UpdateBook(Guid id, [FromForm] UpdateBookRequest request)
    {
        return await _bookService.UpdateBook(id, request);
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBook(Guid id)
    {
        return await _bookService.DeleteBook(id);
    }
    
    [HttpGet("search")]
    public async Task<ActionResult<List<BookResponse>>> SearchBooks([FromQuery] string query)
    {
        return await _bookService.SearchBooks(query);
    }
    
    [HttpGet("coming-soon")]
    public async Task<ActionResult<List<BookResponse>>> GetComingSoonBooks()
    {
        return await _bookService.GetComingSoonBooks();
    }
    
    [HttpGet("best-sellers")]
    public async Task<ActionResult<List<BookResponse>>> GetBestSellerBooks([FromQuery] int limit = 10)
    {
        return await _bookService.GetBestSellerBooks(limit);
    }
}
