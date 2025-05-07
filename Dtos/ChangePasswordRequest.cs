using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; }

    [Required, MinLength(8), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
         ErrorMessage = "New password must be at least 8 characters and contain uppercase, lowercase, number, and special character")]
    public string NewPassword { get; set; }
}