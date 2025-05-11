namespace Backend.Dtos.Bannner;

public class CreateBannerAnnouncementResponse
{
    public Guid Id { get; set; }
    public string Heading { get; set; }
    public string Message { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}