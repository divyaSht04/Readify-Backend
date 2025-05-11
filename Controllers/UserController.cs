using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/user")]
public class UserController
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPut("profile/{userId}")]
    [Authorize]
    public async Task<ActionResult> EditProfile(string userId, [FromForm] EditProfileRequest request)
    {
        return await _userService.EditProfile(userId, request);
    }
}