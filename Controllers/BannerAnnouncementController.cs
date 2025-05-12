using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Backend.Dtos.Bannner;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/banner-announcements")]
    [Authorize(Roles = "ADMIN")]
    public class BannerAnnouncementController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannerAnnouncementController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

       
        [HttpPost]
        public async Task<ActionResult<CreateBannerAnnouncementResponse>> SetBannerAnnouncement([FromBody] CreateBannerAnnouncementRequest request)
        {
            var result = await _bannerService.SetBannerAnnouncement(request);
            return result;
        }

          [HttpDelete]
        public async Task<ActionResult> RemoveBannerAnnouncement()
        {
            var result = await _bannerService.RemoveBannerAnnouncement();
            return result;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CreateBannerAnnouncementResponse>>> GetActiveBannerAnnouncements()
        {
            var result = await _bannerService.GetActiveBannerAnnouncements();
            return result;
        }
    }
}