using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly IEmailVerificationService _emailVerificationService;

    public EmailController(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost("send-otp/{userId}")]
    public async Task<ActionResult> SendOtp(string userId)
    {
        var user = await _emailVerificationService.CheckEmailVerificationStatusAsync(userId);
        if (user is not OkObjectResult okResult)
            return user;

        var userData = okResult.Value as dynamic;
        if (userData == null || userData.isEmailVerified)
            return new BadRequestObjectResult("Email already verified or invalid user");

        var userEntity = await _emailVerificationService.GenerateAndSendOtpAsync(userId, null, null);
        return userEntity;
    }

    [HttpPost("verify-otp/{userId}")]
    public async Task<ActionResult> VerifyOtp(string userId, [FromBody] VerifyOtpRequest request)
    {
        return await _emailVerificationService.VerifyOtpAsync(userId, request.Otp);
    }

    [HttpGet("status/{userId}")]
    public async Task<ActionResult> CheckEmailVerificationStatus(string userId)
    {
        return await _emailVerificationService.CheckEmailVerificationStatusAsync(userId);
    }
}