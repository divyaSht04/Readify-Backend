using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model;

public class Whitelist
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("UserId")]
    public Users? User { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
} 