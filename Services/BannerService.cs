using Backend.Context;
using Backend.Dtos.Bannner;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BannerService : IBannerService
{
    private readonly ApplicationDBContext _context;
    
    public BannerService(ApplicationDBContext context)
    {
        _context = context;
    }
    
            public async Task<ActionResult<CreateBannerAnnouncementResponse>> SetBannerAnnouncement(CreateBannerAnnouncementRequest request)
{
    var existingAnnouncements = await _context.BannerAnnouncements
        .Where(ba => ba.StartDate <= request.EndDate && ba.EndDate >= request.StartDate)
        .ToListAsync();

    if (existingAnnouncements.Any())
    {
        _context.BannerAnnouncements.RemoveRange(existingAnnouncements);
    }

    var bannerAnnouncement = new BannerAnnouncement
    {
        Id = Guid.NewGuid(),
        Heading = request.Heading,
        Message = request.Message,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    _context.BannerAnnouncements.Add(bannerAnnouncement);
    await _context.SaveChangesAsync();

    return new CreateBannerAnnouncementResponse
    {
        Id = bannerAnnouncement.Id,
        Heading = request.Heading,
        Message = bannerAnnouncement.Message,
        StartDate = bannerAnnouncement.StartDate,
        EndDate = bannerAnnouncement.EndDate,
        CreatedAt = bannerAnnouncement.CreatedAt,
        UpdatedAt = bannerAnnouncement.UpdatedAt
    };
}

        public async Task<ActionResult> RemoveBannerAnnouncement()
{
    var activeAnnouncements = await _context.BannerAnnouncements
        .Where(ba => ba.StartDate <= DateTime.UtcNow && ba.EndDate >= DateTime.UtcNow)
        .ToListAsync();

    if (!activeAnnouncements.Any())
    {
        return new NotFoundObjectResult("No active banner announcement found.");
    }

    _context.BannerAnnouncements.RemoveRange(activeAnnouncements);
    await _context.SaveChangesAsync();

    return new OkObjectResult(new { message = "Banner announcement removed successfully" });
}

        public async Task<ActionResult<List<CreateBannerAnnouncementResponse>>> GetActiveBannerAnnouncements()
{
    var now = DateTime.UtcNow;
    var activeAnnouncements = await _context.BannerAnnouncements
        .Where(ba => ba.StartDate <= now && ba.EndDate >= now)
        .Select(ba => new CreateBannerAnnouncementResponse
        {
            Id = ba.Id,
            Heading = ba.Heading,
            Message = ba.Message,
            StartDate = ba.StartDate,
            EndDate = ba.EndDate,
            CreatedAt = ba.CreatedAt,
            UpdatedAt = ba.UpdatedAt
        })
        .ToListAsync();

    return activeAnnouncements;
}
        }
