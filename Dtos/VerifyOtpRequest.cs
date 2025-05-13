using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class VerifyOtpRequest
{
    [Required]
    public string? Email { get; set; }
    
    [Required]
    public string? OtpCode { get; set; }
} 