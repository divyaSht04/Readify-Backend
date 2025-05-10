using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Booking;

public class CreateBookingRequest
{
    [Required]
    public Guid BookID { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
