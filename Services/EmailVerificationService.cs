using Backend;
using Backend.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly ApplicationDBContext _context;
    private readonly IEmailSenderService _emailSenderService;

    public EmailVerificationService(ApplicationDBContext context, IEmailSenderService emailSenderService)
    {
        _context = context;
        _emailSenderService = emailSenderService;
    }

    public async Task<ActionResult> GenerateAndSendOtpAsync(string userId, string email, string name)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            return new BadRequestObjectResult("Invalid user ID");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        if (user.IsEmailVerified)
            return new BadRequestObjectResult("Email already verified");

        // Generate OTP
        var otp = new Random().Next(100000, 999999).ToString(); // 6-digit OTP
        var otpExpiry = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

        user.Otp = otp;
        user.OtpExpiry = otpExpiry;
        await _context.SaveChangesAsync();

        // Send OTP email
        try
        {
            await _emailSenderService.SendOtpEmailAsync(email, name, otp);
            return new OkObjectResult(new { message = "OTP sent to your email. Please verify within 10 minutes." });
        }
        catch (Exception ex)
        {
            // Log the error (use a proper logging framework in production)
            Console.WriteLine($"Failed to send OTP email: {ex.Message}");
            return new ObjectResult(new { error = "Failed to send OTP email. Please try again later." })
            {
                StatusCode = 500
            };
        }
    }

    public async Task<ActionResult> VerifyOtpAsync(string userId, string otp)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            return new BadRequestObjectResult("Invalid user ID");

        if (string.IsNullOrEmpty(otp))
            return new BadRequestObjectResult("OTP is required");

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

    public async Task<ActionResult> CheckEmailVerificationStatusAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            return new BadRequestObjectResult("Invalid user ID");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedUserId);
        if (user == null)
            return new NotFoundObjectResult("User not found");

        return new OkObjectResult(new { isEmailVerified = user.IsEmailVerified });
    }
}