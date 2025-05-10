using System.ComponentModel.DataAnnotations;
using Backend.enums;

namespace Backend.Dtos.BookAccolade;

public class CreateBookAccoladeRequest
{
    [Required]
    public AccoladeType AccoladeType { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    public DateTime AwardedDate { get; set; }
    
    [StringLength(100)]
    public string Category { get; set; }
    
    [Required]
    public Guid BookID { get; set; }
}
