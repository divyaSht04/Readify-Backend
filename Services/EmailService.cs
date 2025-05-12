using System.Net.Mail;
using Backend.Dtos.Order;

namespace Backend.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpServer = _configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        _fromEmail = _configuration["EmailSettings:FromEmail"];
    }

    public async Task SendOrderConfirmationEmailAsync(string toEmail, string claimCode, decimal totalAmount, List<OrderItemResponse> items)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Order Confirmation - Readify",
            Body = GenerateEmailBody(claimCode, totalAmount, items),
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }

    private string GenerateEmailBody(string claimCode, decimal totalAmount, List<OrderItemResponse> items)
    {
        var itemsHtml = string.Join("", items.Select(item => $@"
            <tr>
                <td>{item.BookTitle}</td>
                <td>{item.BookAuthor}</td>
                <td>{item.Quantity}</td>
                <td>${item.UnitPrice:F2}</td>
                <td>${item.TotalPrice:F2}</td>
            </tr>"));

        return $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Thank you for your order!</h2>
                    <p>Your order has been successfully placed.</p>
                    <p><strong>Claim Code:</strong> {claimCode}</p>
                    <p><strong>Total Amount:</strong> ${totalAmount:F2}</p>
                    
                    <h3>Order Details:</h3>
                    <table border='1' cellpadding='5' style='border-collapse: collapse;'>
                        <tr style='background-color: #f2f2f2;'>
                            <th>Book Title</th>
                            <th>Author</th>
                            <th>Quantity</th>
                            <th>Unit Price</th>
                            <th>Total</th>
                        </tr>
                        {itemsHtml}
                    </table>
                    
                    <p>Please keep this claim code for your records. You will need it to claim your books.</p>
                    <p>Thank you for choosing Readify!</p>
                </body>
            </html>";
    }
}
