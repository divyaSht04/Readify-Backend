using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model
{
    [Table("Discounts")]
    public class Discount
    {
        [Key]
        public Guid Id { get; set; }

        [Required, Range(0, 100)] public decimal Percentage { get; set; } = 0;

        [Required]
        public string DiscountName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool OnSale { get; set; } = false;

        public Guid? BookId { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}