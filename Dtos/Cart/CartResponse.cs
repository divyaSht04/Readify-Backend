namespace Backend.Dtos.Cart;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal TotalPrice { get; set; } = 0;
    public decimal OriginalTotalPrice { get; set; } = 0;// Original price before volume discount
    public decimal VolumeDiscountAmount { get; set; } = 0; // Amount saved from volume discount
    public bool HasVolumeDiscount { get; set; } // Whether volume discount was applied
    public string VolumeDiscountMessage { get; set; } // Explains the volume discount
    public bool HasLoyaltyDiscount { get; set; } // Whether loyalty discount was applied
    public decimal LoyaltyDiscountAmount { get; set; } = 0; // Amount saved from loyalty discount
    public string LoyaltyDiscountMessage { get; set; } // Explains the loyalty discount
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; }
    public string BookAuthor { get; set; }
    public decimal BookPrice { get; set; } = 0;
    public decimal? DiscountedPrice { get; set; } = 0;
    public decimal? DiscountPercentage { get; set; } = 0;
    public bool OnSale { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}