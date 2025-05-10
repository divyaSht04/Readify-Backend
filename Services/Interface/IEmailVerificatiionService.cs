using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IEmailVerificationService
{
    Task<ActionResult> GenerateAndSendOtpAsync(string userId, string email, string name);
    Task<ActionResult> VerifyOtpAsync(string userId, string otp);
    Task<ActionResult> CheckEmailVerificationStatusAsync(string userId);
}