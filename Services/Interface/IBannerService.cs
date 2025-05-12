using Backend.Dtos.Bannner;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IBannerService
{
    Task<ActionResult<CreateBannerAnnouncementResponse>> SetBannerAnnouncement(CreateBannerAnnouncementRequest request);
    Task<ActionResult> RemoveBannerAnnouncement();
    Task<ActionResult<List<CreateBannerAnnouncementResponse>>> GetActiveBannerAnnouncements();

}