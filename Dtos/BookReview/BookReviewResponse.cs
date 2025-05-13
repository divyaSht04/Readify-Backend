namespace Backend.Dtos.BookReview;

public class BookReviewResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserImage { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 