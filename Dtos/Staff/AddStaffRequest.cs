using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class AddStaffRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        
        [Required]
        public string? FullName { get; set; }
    
        [Required]
        public string? PhoneNumber { get; set; }
    
        [Required]
        public string? Address { get; set; }

    }
}