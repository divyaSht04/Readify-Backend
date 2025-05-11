namespace Backend.Dtos.Cart;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public decimal BookPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 