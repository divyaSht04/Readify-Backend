namespace Backend.Dtos.Order;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ClaimCode { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OriginalTotalAmount { get; set; }
    public decimal? VolumeDiscountAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; }
}