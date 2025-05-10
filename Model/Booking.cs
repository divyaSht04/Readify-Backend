using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.enums;

namespace Backend;

public class Booking
{
    [Key]
    public Guid ID { get; set; }
    
    [Required]
    public Guid UserID { get; set; }
    
    [Required]
    public Guid BookID { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }
    
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("UserID")]
    public virtual Users User { get; set; }
    
    [ForeignKey("BookID")]
    public virtual Book Book { get; set; }
}
