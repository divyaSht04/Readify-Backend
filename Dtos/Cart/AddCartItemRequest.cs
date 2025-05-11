using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Cart;

public class AddCartItemRequest
{
    [Required]
    public Guid BookId { get; set; }
} 