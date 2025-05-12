using System;
using System.Collections.Generic;

namespace Backend.Dtos;

public class BookResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public DateTime? PublishedDate { get; set; }
    public int StockQuantity { get; set; }
    public bool IsComingSoon { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Category { get; set; }
    public string? Image { get; set; }
}
