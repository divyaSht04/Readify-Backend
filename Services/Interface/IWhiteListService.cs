using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IWhiteListService
{
    Task<ActionResult<BookResponse>> AddWhiteListAsync(Guid bookId, Guid userId);
    Task<ActionResult<BookResponse>> RemoveFromWhitelist(Guid bookId, Guid userId);
    Task<ActionResult<List<BookResponse>>> GetUserWhitelist(Guid userId);
}