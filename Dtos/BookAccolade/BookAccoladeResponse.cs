using Backend.enums;

namespace Backend.Dtos.BookAccolade;

public class BookAccoladeResponse
{
    public Guid ID { get; set; }
    public AccoladeType AccoladeType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime AwardedDate { get; set; }
    public string Category { get; set; }
    public Guid BookID { get; set; }
    public string BookTitle { get; set; }
    public string Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
