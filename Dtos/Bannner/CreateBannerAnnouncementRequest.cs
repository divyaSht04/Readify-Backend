using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Bannner;


    public class CreateBannerAnnouncementRequest
    {
        [Required]
        public string Heading { get; set; }
        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
