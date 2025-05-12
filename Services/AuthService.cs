using System;
using System.Threading.Tasks;
using Backend.Context;
using Backend.Dtos;
using Backend.enums;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDBContext _context;
    private readonly JwtUtils _jwtUtils;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;

    public AuthService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration, IFileService fileService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
        _fileService = fileService;
    }

    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return new UnauthorizedObjectResult("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return new UnauthorizedObjectResult("Invalid email or password");

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new ConflictObjectResult("User with this email already exists");
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Console.WriteLine($"Hashed Password: {hashedPassword}"); // Debug output

        // Save the image if provided
        string? imagePath = null;
        if (request.ImageFile != null)
        {
            imagePath = await _fileService.SaveFile(request.ImageFile, "users");
        }

        var user = new Users
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Password = hashedPassword,
            Name = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Image = imagePath,
            Role = Roles.ADMIN
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
            return new BadRequestObjectResult("Invalid client request");

        string? accessToken = request.AccessToken;
        string? refreshToken = request.RefreshToken;

        var principal = _jwtUtils.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            return new BadRequestObjectResult("Invalid access token or refresh token");

        string userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new BadRequestObjectResult("Invalid access token");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return new BadRequestObjectResult("Invalid access token or refresh token");

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult> RevokeToken(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return new UnauthorizedResult();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        if (user == null)
            return new NotFoundResult();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _context.SaveChangesAsync();

        return new NoContentResult();
    }
    
   
    public async Task<ActionResult> ChangePassword(string userId, ChangePasswordRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            return new BadRequestObjectResult("Invalid client request");

        if (!Guid.TryParse(userId, out var parsedUserId))
            return new BadRequestObjectResult("Invalid user ID");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        // Trim inputs to handle potential whitespace
        var currentPassword = request.CurrentPassword?.Trim();
        var newPassword = request.NewPassword?.Trim();

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            return new UnauthorizedObjectResult("Current password is incorrect");

        // Check if new password is different
        if (currentPassword == newPassword)
            return new BadRequestObjectResult("New password must be different from the current password");

        // Hash new password
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.Updated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Password changed successfully" });
    }


    private async Task<ActionResult<AuthResponse>> GenerateAuthResponse(Users user)
    {
        var token = _jwtUtils.GenerateJwtToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            Convert.ToDouble(_configuration["JWT:RefreshTokenValidityInDays"] ?? "7"));

        await _context.SaveChangesAsync();

        return new OkObjectResult(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["JWT:TokenValidityInMinutes"] ?? "60")),
            UserId = user.Id.ToString(),
            Email = user.Email,
            Role = user.Role.ToString(),
            Name = user.Name
        });
    }
}