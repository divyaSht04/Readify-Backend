using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Dtos;

public class EditProfileRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; }

    [Required, MaxLength(100)]
    public string PhoneNumber { get; set; }

    [Required, MaxLength(100)]
    public string Address { get; set; }

    public IFormFile? ImageFile { get; set; }
}