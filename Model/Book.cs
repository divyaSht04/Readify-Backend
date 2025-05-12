namespace Backend;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Book
{
    [Key] 
    public Guid ID { get; set; }

    [Column("title"), MaxLength(100), Required]
    [StringLength(255)] 
    public string Title { get; set; }

    [Column( name: "author"),MaxLength(20), Required]
    public string Author { get; set; }

    [Column(name: "ISBN"), MaxLength(100), Required]
    [StringLength(13)]
    public string ISBN { get; set; }

    [Column(name:"price" , TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public string Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PublishedDate { get; set; }

    [Column(name:"stock_quantity")]
    public int StockQuantity { get; set; } = 0;

    [Column(name:"is_coming_soon")]
    public bool IsComingSoon { get; set; } = false;

    [Column(name:"release_date")]
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<string> Category { get; set; }

    [Column("image"), MaxLength(255)]
    public string? Image { get; set; }

    [Column(name: "discounted_price", TypeName = "decimal(10,2)")]
    public decimal? DiscountedPrice { get; set; }

    [Column(name: "on_sale")]
    public bool OnSale { get; set; }

    public virtual ICollection<BookAccolade> Accolades { get; set; }
}