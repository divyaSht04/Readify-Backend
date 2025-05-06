using Backend.Context;
using Backend.Dtos;
using Backend.enums;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;


public class AuthService : IAuthService
{
    private readonly ApplicationDBContext _context;
    private readonly JwtUtils _jwtUtils;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
    }

    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return new UnauthorizedObjectResult("Invalid email or password");

        // In a production environment, you should use password hashing
        if (user.Password != request.Password)
            return new UnauthorizedObjectResult("Invalid email or password");

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");
        
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new ConflictObjectResult("User with this email already exists");
        
        var user = new Users
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Password = request.Password, 
            Name = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Image = request.Image,
            Role = Roles.USER 
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
            new BadRequestObjectResult("Invalid access token or refresh token");

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
        await _context.SaveChangesAsync();

        return new NoContentResult();
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