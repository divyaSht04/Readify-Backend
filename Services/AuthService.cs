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
    private readonly IEmailService _emailService;

    public AuthService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration, IFileService fileService, IEmailService emailService)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _configuration = configuration;
        _fileService = fileService;
        _emailService = emailService;
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

        if (!user.IsVerified)
            return new BadRequestObjectResult("Email not verified. Please verify your email to login.");

        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult> Register(RegisterRequest request)
    {
        if (request == null)
            return new BadRequestObjectResult("Invalid client request");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new ConflictObjectResult("User with this email already exists");
        
        // Check if there's already a pending registration for this email
        var existingPending = await _context.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == request.Email);
        if (existingPending != null)
        {
            // If the pending registration is expired, delete it
            if (existingPending.OtpExpiryTime < DateTime.UtcNow)
            {
                _context.PendingRegistrations.Remove(existingPending);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Otherwise, we can just resend the OTP
                await _emailService.SendVerificationOtpEmailAsync(existingPending.Email, existingPending.OtpCode);
                
                return new OkObjectResult(new
                {
                    message = "Registration verification code resent. Please check your email.",
                    email = existingPending.Email
                });
            }
        }
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        Console.WriteLine($"Hashed Password: {hashedPassword}"); // Debug output

        // Save the image if provided
        string? imagePath = null;
        if (request.ImageFile != null)
        {
            imagePath = await _fileService.SaveFile(request.ImageFile, "users");
        }

        // Generate OTP
        string otp = GenerateOtp();
        DateTime otpExpiry = DateTime.UtcNow.AddMinutes(10);

        // Create pending registration
        var pendingRegistration = new PendingRegistration
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            HashedPassword = hashedPassword,
            ImagePath = imagePath,
            OtpCode = otp,
            OtpExpiryTime = otpExpiry
        };

        await _context.PendingRegistrations.AddAsync(pendingRegistration);
        await _context.SaveChangesAsync();

        // Send OTP email
        await _emailService.SendVerificationOtpEmailAsync(pendingRegistration.Email, otp);

        // Return email for verification page
        return new OkObjectResult(new
        {
            message = "Registration initiated. Please verify your email.",
            email = pendingRegistration.Email
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
        
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
            return new UnauthorizedObjectResult("Current password is incorrect");

        // Check if new password is different
        if (currentPassword == newPassword)
            return new BadRequestObjectResult("New password must be different from the current password !");

        // Hash new password
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.Updated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new OkObjectResult(new { message = "Password changed successfully" });
    }
    
    public async Task<ActionResult<AuthResponse>> VerifyOtp(VerifyOtpRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OtpCode))
            return new BadRequestObjectResult("Invalid client request");

        // Check for pending registration
        var pendingRegistration = await _context.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == request.Email);
        if (pendingRegistration == null)
            return new NotFoundObjectResult("No pending registration found for this email");

        // Validate OTP
        if (pendingRegistration.OtpCode != request.OtpCode)
            return new BadRequestObjectResult("Invalid OTP code");

        if (pendingRegistration.OtpExpiryTime < DateTime.UtcNow)
            return new BadRequestObjectResult("OTP has expired. Please request a new one.");

        // Create the actual user
        var user = new Users
        {
            Id = Guid.NewGuid(),
            Email = pendingRegistration.Email,
            Password = pendingRegistration.HashedPassword,
            Name = pendingRegistration.FullName,
            PhoneNumber = pendingRegistration.PhoneNumber,
            Address = pendingRegistration.Address,
            Image = pendingRegistration.ImagePath,
            Role = Roles.USER,
            IsVerified = true, // User is already verified
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        
        // Remove the pending registration
        _context.PendingRegistrations.Remove(pendingRegistration);
        await _context.SaveChangesAsync();

        // Generate auth response with tokens
        return await GenerateAuthResponse(user);
    }

    public async Task<ActionResult> ResendOtp(string email)
    {
        if (string.IsNullOrEmpty(email))
            return new BadRequestObjectResult("Email is required");

        // Check for pending registration instead of user
        var pendingRegistration = await _context.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == email);
        if (pendingRegistration == null)
            return new NotFoundObjectResult("No pending registration found for this email");

        // Generate new OTP
        string otp = GenerateOtp();
        DateTime otpExpiry = DateTime.UtcNow.AddMinutes(10);

        // Update pending registration
        pendingRegistration.OtpCode = otp;
        pendingRegistration.OtpExpiryTime = otpExpiry;

        await _context.SaveChangesAsync();

        // Send OTP email
        await _emailService.SendVerificationOtpEmailAsync(pendingRegistration.Email, otp);

        return new OkObjectResult(new { message = "OTP sent successfully" });
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
    
    private string GenerateOtp()
    {
        // Generate a 6-digit numeric OTP
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}