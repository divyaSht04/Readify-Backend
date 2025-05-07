using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class EditProfileRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; }

    [Required, MaxLength(100)]
    public string PhoneNumber { get; set; }

    [Required, MaxLength(100)]
    public string Address { get; set; }

    [MaxLength(100)]
    public string? Image { get; set; }
}