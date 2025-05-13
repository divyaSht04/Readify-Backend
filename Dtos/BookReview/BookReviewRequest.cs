using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.BookReview;

public class BookReviewRequest
{
    [Required]
    public Guid BookId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }
    
    [StringLength(1000, ErrorMessage = "Review text cannot exceed 1000 characters")]
    public string? ReviewText { get; set; }
} 