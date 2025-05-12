namespace Backend.Dtos.Order;

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 