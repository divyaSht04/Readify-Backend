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
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration, IEmailVerificationService emailVerificationService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return new BadRequestObjectResult("Invalid client request");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return new UnauthorizedObjectResult("Invalid email or password");

        if (!user.IsEmailVerified)
            return new UnauthorizedObjectResult("Email not verified. Please verify your email using the OTP sent to your inbox.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return new UnauthorizedObjectResult("Invalid email or password");

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        // Validate email domain whitelist
        var allowedDomains = _configuration.GetSection("Auth:AllowedEmailDomains").Get<List<string>>();
        if (allowedDomains != null && allowedDomains.Any())
        {
            var emailDomain = request.Email?.Split('@').LastOrDefault()?.ToLower();
            if (string.IsNullOrEmpty(emailDomain) || !allowedDomains.Contains(emailDomain))
            {
                return new ObjectResult(new { error = "Email domain not allowed" })
                {
                    StatusCode = 403 // Forbidden
                };
            }
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new ConflictObjectResult("User with this email already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new Users
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Password = hashedPassword,
            Name = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Image = request.Image,
            Role = Roles.USER,
            IsEmailVerified = false
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Generate and send OTP using EmailVerificationService
        var otpResult = await _emailVerificationService.GenerateAndSendOtpAsync(user.Id.ToString(), user.Email, user.Name);
        if (otpResult is not OkObjectResult)
            return otpResult;

        return new OkObjectResult(new
        {
            message = "Registration successful. Please verify your email using the OTP sent to your inbox.",
            userId = user.Id.ToString()
        });
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

        if (!user.IsEmailVerified)
            return new UnauthorizedObjectResult("Email not verified. Please verify your email using the OTP sent to your inbox.");

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

    public async Task<ActionResult> VerifyOtp(string userId, string otp)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otp))
            return new BadRequestObjectResult("Invalid user ID or OTP");

        if (!Guid.TryParse(userId, out var parsedUserId))
            return new BadRequestObjectResult("Invalid user ID");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        if (user.IsEmailVerified)
            return new BadRequestObjectResult("Email already verified");

        if (user.Otp != otp)
            return new BadRequestObjectResult("Invalid OTP");

        if (user.OtpExpiry < DateTime.UtcNow)
            return new BadRequestObjectResult("OTP has expired");

        user.IsEmailVerified = true;
        user.Otp = null;
        user.OtpExpiry = null;
        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Email verified successfully" });
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