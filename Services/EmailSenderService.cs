using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly IConfiguration _configuration;

    public EmailSenderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpEmailAsync(string email, string name, string otp)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"]);
        var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]);
        var smtpUsername = _configuration["Smtp:Username"];
        var smtpPassword = _configuration["Smtp:Password"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUsername, "Your App Name"),
            Subject = "Email Verification OTP",
            Body = $"Dear {name},\n\nYour OTP for email verification is: {otp}\n\nThis OTP is valid for 10 minutes.\n\nBest regards,\nYour App Team",
            IsBodyHtml = false
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}