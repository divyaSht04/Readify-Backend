using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IWhiteListService
{
    Task<ActionResult<WhitelistResponseDto>> AddWhiteListAsync(Guid bookId, Guid userId);
    Task<ActionResult<WhitelistResponseDto>> RemoveFromWhitelist(Guid bookId, Guid userId);
    Task<ActionResult<WhitelistResponseDto>> GetUserWhitelist(Guid userId);
}