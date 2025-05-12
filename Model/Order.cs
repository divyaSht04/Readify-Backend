using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string ClaimCode { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public decimal OriginalTotalAmount { get; set; }
    
    public decimal? VolumeDiscountAmount { get; set; }
    
    [Required]
    public string Status { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    public virtual ICollection<OrderItem> Items { get; set; }
    
    [ForeignKey("UserId")]
    public virtual Users User { get; set; }
}