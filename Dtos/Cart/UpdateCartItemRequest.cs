using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Cart;

public class UpdateCartItemRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
} 