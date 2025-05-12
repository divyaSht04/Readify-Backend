using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class AddToWhitelistDto
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid BookId { get; set; }
}

public class WhitelistResponseDto
{
    public int Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string? BookImage { get; set; }
    public DateTime CreatedAt { get; set; }
} 