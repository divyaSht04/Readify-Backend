using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class VerifyOtpRequest
{
    [Required]
    public string Otp { get; set; }
}