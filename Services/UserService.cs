using Backend.Context;
using Backend.Dtos;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService : IUserService
{

    private readonly ApplicationDBContext _context;
    private readonly IFileService _fileService;

    public UserService(ApplicationDBContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<ActionResult> EditProfile(string userId, EditProfileRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        if (request.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(user.Image))
            {
                _fileService.DeleteFile(user.Image);
            }

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