using Backend.Context;
using Backend.Dtos;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService : IUserService
{
    
    private readonly ApplicationDBContext _context;
    private readonly JwtUtils _jwtUtils;
    private readonly IConfiguration _configuration;

    public UserService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
    }
    
    public async Task<ActionResult> EditProfile(string userId, EditProfileRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        user.Name = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address;
        user.Image = request.Image;
        user.Updated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new
        {
            message = "Profile updated successfully",
            user = new
            {
                user.Id,
                user.Email,
                user.Name,
                user.PhoneNumber,
                user.Address,
                user.Image,
                user.Role
            }
        });
    }
}
    
