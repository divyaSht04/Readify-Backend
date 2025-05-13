using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model;

public class BookReview
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [StringLength(1000)]
    public string? ReviewText { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("BookId")]
    public virtual Book Book { get; set; }
    
    [ForeignKey("UserId")]
    public virtual Users User { get; set; }
} 