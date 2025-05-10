namespace Backend.Services;

public interface IEmailSenderService
{
    Task SendOtpEmailAsync(string email, string name, string otp);
}