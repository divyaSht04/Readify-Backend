using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Dtos;

public class RegisterRequest
{
    [Required]
    public string? Email { get; set; }
    
    [Required]
    public string? Password { get; set; }
    
    [Required]
    public string? FullName { get; set; }
    
    [Required]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public string? Address { get; set; }
    
    public IFormFile? ImageFile { get; set; }
}