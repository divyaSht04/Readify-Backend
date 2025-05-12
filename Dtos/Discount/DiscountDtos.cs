namespace Backend.Dtos
{
    public class CreateDiscountRequest
    {
        public string DiscountName { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool OnSale { get; set; }
    }

    public class DiscountResponse
    {
        public string DiscountName { get; set; }
        public Guid Id { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool OnSale { get; set; }
        public Guid? BookId { get; set; }
        public string BookTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}