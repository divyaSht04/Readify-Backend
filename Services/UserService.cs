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
    private readonly IFileService _fileService;

    public UserService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration, IFileService fileService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
        _fileService = fileService;
    }
    
    public async Task<ActionResult> EditProfile(string userId, EditProfileRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        // Handle image upload
        if (request.ImageFile != null)
        {
            // Delete old image if it exists
            if (!string.IsNullOrEmpty(user.Image))
            {
                _fileService.DeleteFile(user.Image);
            }
            
            // Save new image
            string? imagePath = await _fileService.SaveFile(request.ImageFile, "users");
            user.Image = imagePath;
        }

        user.Name = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address;
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
    
