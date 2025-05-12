using Backend.Context;
using Backend.Dtos;
using Backend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("/whitelist")]
[Authorize]
public class WhitelistController : ControllerBase
{
    private readonly ApplicationDBContext _context;
    private readonly IWhiteListService _whiteListService;

    public WhitelistController(ApplicationDBContext context, IWhiteListService whiteListService)
    {
        _context = context;
        _whiteListService = whiteListService;
    }

    [HttpGet("userId/{userId}")]
    public async Task<ActionResult<List<BookResponse>>> GetUserWhitelist(Guid userId)
    {
        return await _whiteListService.GetUserWhitelist(userId);
    }

    [HttpPost("/bookId/{bookId}/userId/{userId}")]
    public async Task<ActionResult<BookResponse>> AddToWhitelist(Guid bookId, Guid userId)
    {
        return await _whiteListService.AddWhiteListAsync(bookId, userId);
    }

    [HttpDelete("{bookId}/user/{userId}")]
    public async Task<ActionResult<BookResponse>> RemoveFromWhitelist(Guid bookId, Guid userId)
    {
       return await _whiteListService.RemoveFromWhitelist(bookId, userId);
    }
} 