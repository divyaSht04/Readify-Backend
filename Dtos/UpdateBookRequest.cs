using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos;

public class UpdateBookRequest
{
    [StringLength(255)]
    public string Title { get; set; }
    
    [StringLength(13)]
    public string Author{ get; set; }
    
    [StringLength(13)]
    public string ISBN { get; set; }
    
    public decimal? Price { get; set; }
    
    public string Description { get; set; }
    
    public DateTime? PublishedDate { get; set; }
    
    public int? StockQuantity { get; set; }
    
    public List<string> Category { get; set; }
}
