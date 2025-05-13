using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class ResendOtpRequest
{
    [Required]
    public string? Email { get; set; }
} 