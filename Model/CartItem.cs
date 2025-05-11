using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model;

public class CartItem
{
    [Required]
    public Guid BookId { get; set; }
    [Required]
    public Guid CartId { get; set; }
    [Required]
    public int Quantity { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation properties
    [ForeignKey("CartId")]
    public virtual Cart Cart { get; set; }
    [ForeignKey("BookId")]
    public virtual Book Book { get; set; }
} 