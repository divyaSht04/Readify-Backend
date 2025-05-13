using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{userId}")]
    [Authorize]
    public async Task<ActionResult> GetUserById(string userId)
    {
        return await _userService.GetUserById(userId);
    }
    
    [HttpPut("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult> EditProfile(string userId, [FromForm] EditProfileRequest request)
    {
        return await _userService.EditProfile(userId, request);
    }
}