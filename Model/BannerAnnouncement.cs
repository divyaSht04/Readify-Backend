using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("BannerAnnouncements")]
    public class BannerAnnouncement
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Heading { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}