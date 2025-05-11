using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("GlobalDiscounts")]
    public class GlobalDiscount
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required, Range(0, 100)]
        public decimal Percentage { get; set; }
        [Required]
        public string DiscountName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool OnSale { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}