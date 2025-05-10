using Backend.enums;
using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Booking;

public class UpdateBookingRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int? Quantity { get; set; }
    
    public BookingStatus? Status { get; set; }
    
    public DateTime? PickupDate { get; set; }
}
