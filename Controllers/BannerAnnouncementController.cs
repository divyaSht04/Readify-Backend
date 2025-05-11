using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend.Dtos.Bannner;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/admin/banner-announcements")]
    [Authorize(Roles = "ADMIN")] // Restrict to admin users
    public class BannerAnnouncementController : ControllerBase
    {
        private readonly AdminService _adminService;

        public BannerAnnouncementController(AdminService adminService)
        {
            _adminService = adminService;
        }

       
        [HttpPost]
        public async Task<ActionResult<CreateBannerAnnouncementResponse>> SetBannerAnnouncement([FromBody] CreateBannerAnnouncementRequest request)
        {
            var result = await _adminService.SetBannerAnnouncement(request);
            return result;
        }

          [HttpDelete]
        public async Task<ActionResult> RemoveBannerAnnouncement()
        {
            var result = await _adminService.RemoveBannerAnnouncement();
            return result;
        }

        
        [HttpGet]
        [AllowAnonymous] // Allow public access to view active announcements
        public async Task<ActionResult<List<CreateBannerAnnouncementResponse>>> GetActiveBannerAnnouncements()
        {
            var result = await _adminService.GetActiveBannerAnnouncements();
            return result;
        }
    }
}