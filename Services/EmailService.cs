using System.Net.Mail;
using System.Text;
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
    private readonly string _templatePath;

    public EmailService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _smtpServer = _configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        _fromEmail = _configuration["EmailSettings:FromEmail"];
        _templatePath = Path.Combine(env.ContentRootPath, "EmailTemplates");
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
            Body = await GenerateOrderConfirmationEmailBody(claimCode, totalAmount, items),
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
    
    public async Task SendVerificationOtpEmailAsync(string toEmail, string otp)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Account Verification - Readify",
            Body = await GenerateVerificationOtpEmailBody(otp),
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }

    private async Task<string> GenerateVerificationOtpEmailBody(string otp)
    {
        string templatePath = Path.Combine(_templatePath, "VerificationOtp.html");
        string template = await File.ReadAllTextAsync(templatePath);
        
        return template
            .Replace("{{OTP_CODE}}", otp)
            .Replace("{{CURRENT_YEAR}}", DateTime.Now.Year.ToString());
    }

    public async Task SendOrderVerificationEmailAsync(string toEmail, string claimCode, decimal totalAmount, List<OrderItemResponse> items)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = "Order Verified - Readify",
            Body = await GenerateOrderVerificationEmailBody(claimCode, totalAmount, items),
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }

    private async Task<string> GenerateOrderConfirmationEmailBody(string claimCode, decimal totalAmount, List<OrderItemResponse> items)
    {
        string templatePath = Path.Combine(_templatePath, "OrderConfirmation.html");
        string template = await File.ReadAllTextAsync(templatePath);
        
        // Calculate original total (before discounts)
        decimal originalTotal = items.Sum(item => item.UnitPrice * item.Quantity);
        
        // Generate HTML for order items
        var itemsHtml = new StringBuilder();
        foreach (var item in items)
        {
            string rowClass = item.DiscountedPrice.HasValue ? "discount-row" : "";
            itemsHtml.Append($"<tr class=\"{rowClass}\">\n");
            itemsHtml.Append($"<td class=\"book-title\">{item.BookTitle}</td>\n");
            itemsHtml.Append($"<td>{item.BookAuthor}</td>\n");
            itemsHtml.Append($"<td>{item.Quantity}</td>\n");
            
            if (item.DiscountedPrice.HasValue)
            {
                itemsHtml.Append($"<td><s>${item.UnitPrice:F2}</s> <span class=\"discount\">${item.DiscountedPrice:F2}</span></td>\n");
            }
            else
            {
                itemsHtml.Append($"<td>${item.UnitPrice:F2}</td>\n");
            }
            
            itemsHtml.Append($"<td>${item.TotalPrice:F2}</td>\n");
            itemsHtml.Append("</tr>\n");
        }
        
        // Calculate discount amounts
        decimal volumeDiscountAmount = originalTotal - totalAmount;
        string volumeDiscountHtml = "";
        if (volumeDiscountAmount > 0)
        {
            volumeDiscountHtml = $"<tr class=\"discount-row\">\n" +
                                 $"<td>Volume Discount:</td>\n" +
                                 $"<td class=\"discount\">-${volumeDiscountAmount:F2}</td>\n" +
                                 $"</tr>\n";
        }
        
        // Replace placeholders in the template
        return template
            .Replace("{{ORDER_DATE}}", DateTime.Now.ToString("MMMM dd, yyyy"))
            .Replace("{{CLAIM_CODE}}", claimCode)
            .Replace("{{ORDER_ITEMS}}", itemsHtml.ToString())
            .Replace("{{ORIGINAL_TOTAL}}", originalTotal.ToString("F2"))
            .Replace("{{VOLUME_DISCOUNT}}", volumeDiscountHtml)
            .Replace("{{LOYALTY_DISCOUNT}}", "")
            .Replace("{{TOTAL_AMOUNT}}", totalAmount.ToString("F2"))
            .Replace("{{CURRENT_YEAR}}", DateTime.Now.Year.ToString());
    }

    private async Task<string> GenerateOrderVerificationEmailBody(string claimCode, decimal totalAmount, List<OrderItemResponse> items)
    {
        string templatePath = Path.Combine(_templatePath, "OrderVerification.html");
        string template = await File.ReadAllTextAsync(templatePath);
        
        // Calculate original total (before discounts)
        decimal originalTotal = items.Sum(item => item.UnitPrice * item.Quantity);
        
        // Generate HTML for order items
        var itemsHtml = new StringBuilder();
        foreach (var item in items)
        {
            string rowClass = item.DiscountedPrice.HasValue ? "discount-row" : "";
            itemsHtml.Append($"<tr class=\"{rowClass}\">\n");
            itemsHtml.Append($"<td class=\"book-title\">{item.BookTitle}</td>\n");
            itemsHtml.Append($"<td>{item.BookAuthor}</td>\n");
            itemsHtml.Append($"<td>{item.Quantity}</td>\n");
            itemsHtml.Append($"<td>${item.UnitPrice:F2}</td>\n");
            
            if (item.DiscountedPrice.HasValue)
            {
                itemsHtml.Append($"<td class=\"discount\">${item.DiscountedPrice:F2} ({item.DiscountPercentage}% off)</td>\n");
            }
            else
            {
                itemsHtml.Append($"<td>N/A</td>\n");
            }
            
            itemsHtml.Append($"<td>${item.TotalPrice:F2}</td>\n");
            itemsHtml.Append("</tr>\n");
        }
        
        // Calculate discount amounts
        decimal volumeDiscountAmount = originalTotal - totalAmount;
        string volumeDiscountHtml = "";
        if (volumeDiscountAmount > 0)
        {
            volumeDiscountHtml = $"<tr class=\"discount-row\">\n" +
                                 $"<td>Volume Discount:</td>\n" +
                                 $"<td class=\"discount\">-${volumeDiscountAmount:F2}</td>\n" +
                                 $"</tr>\n";
        }
        
        // Generate discount details section
        var discountDetailsHtml = new StringBuilder();
        if (volumeDiscountAmount > 0)
        {
            discountDetailsHtml.Append("<div class=\"discount-details\">\n");
            discountDetailsHtml.Append("<h3>Your Savings:</h3>\n");
            discountDetailsHtml.Append("<ul>\n");
            
            // Item-specific discounts
            var discountedItems = items.Where(i => i.DiscountedPrice.HasValue).ToList();
            if (discountedItems.Any())
            {
                foreach (var item in discountedItems)
                {
                    decimal savedAmount = (item.UnitPrice - item.DiscountedPrice.Value) * item.Quantity;
                    discountDetailsHtml.Append($"<li><strong>{item.BookTitle}</strong>: Saved <span class=\"savings-highlight\">${savedAmount:F2}</span> with {item.DiscountPercentage}% discount</li>\n");
                }
            }
            
            // Volume discount
            if (volumeDiscountAmount > 0)
            {
                discountDetailsHtml.Append($"<li>Volume Discount: Saved <span class=\"savings-highlight\">${volumeDiscountAmount:F2}</span> by ordering multiple books</li>\n");
            }
            
            discountDetailsHtml.Append("</ul>\n");
            discountDetailsHtml.Append($"<p>Total Savings: <span class=\"savings-highlight\">${volumeDiscountAmount:F2}</span> ({(volumeDiscountAmount / originalTotal * 100):F0}% off original price)</p>\n");
            discountDetailsHtml.Append("</div>\n");
        }
        
        // Replace placeholders in the template
        return template
            .Replace("{{ORDER_DATE}}", DateTime.Now.AddDays(-1).ToString("MMMM dd, yyyy")) // Assuming order was placed a day before verification
            .Replace("{{VERIFICATION_DATE}}", DateTime.Now.ToString("MMMM dd, yyyy"))
            .Replace("{{CLAIM_CODE}}", claimCode)
            .Replace("{{ORDER_ITEMS}}", itemsHtml.ToString())
            .Replace("{{DISCOUNT_DETAILS}}", discountDetailsHtml.ToString())
            .Replace("{{ORIGINAL_TOTAL}}", originalTotal.ToString("F2"))
            .Replace("{{VOLUME_DISCOUNT}}", volumeDiscountHtml)
            .Replace("{{LOYALTY_DISCOUNT}}", "")
            .Replace("{{TOTAL_AMOUNT}}", totalAmount.ToString("F2"))
            .Replace("{{CURRENT_YEAR}}", DateTime.Now.Year.ToString());
    }
}