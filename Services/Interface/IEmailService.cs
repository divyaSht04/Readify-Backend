using Backend.Dtos.Order;

namespace Backend.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(string toEmail, string claimCode, decimal totalAmount, List<OrderItemResponse> items);
    Task SendVerificationOtpEmailAsync(string toEmail, string otp);
}