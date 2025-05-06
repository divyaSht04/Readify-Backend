using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class CreateBookRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; }
    
    [Required]
    [StringLength(13)]
    public string ISBN { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public string Description { get; set; }
    
    public DateTime? PublishedDate { get; set; }
    
    public int StockQuantity { get; set; } = 0;
    
    public List<string> Category { get; set; } = new List<string>();
}
