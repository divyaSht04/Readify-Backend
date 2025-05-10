using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.enums;

namespace Backend;

public class BookAccolade
{
    [Key]
    public Guid ID { get; set; }
    
    [Required]
    public AccoladeType AccoladeType { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime AwardedDate { get; set; }
    
    [StringLength(100)]
    public string Category { get; set; }
    
    // Foreign key relationship
    [Required]
    public Guid BookID { get; set; }
    
    [ForeignKey("BookID")]
    public virtual Book Book { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
