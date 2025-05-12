using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    public decimal? DiscountedPrice { get; set; }
    
    public decimal? DiscountPercentage { get; set; }
    
    [Required]
    public decimal TotalPrice { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
    
    [ForeignKey("BookId")]
    public virtual Book Book { get; set; }
} 