using Backend.enums;

namespace Backend.Dtos.Booking;

public class BookingResponse
{
    public Guid ID { get; set; }
    public Guid UserID { get; set; }
    public string UserName { get; set; }
    public Guid BookID { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public int Quantity { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? PickupDate { get; set; }
}
