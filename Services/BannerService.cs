using Backend.Context;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Dtos.Bannner;
using Backend.Utils;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class BannerService : IBannerService
    {
        private readonly ApplicationDBContext _context;
        private readonly JwtUtils _jwtUtils;
        private readonly IConfiguration _configuration;

        public BannerService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _configuration = configuration;
        }

        public async Task<ActionResult<CreateBannerAnnouncementResponse>> SetBannerAnnouncement(CreateBannerAnnouncementRequest request)
        {
            // Validate dates
            if (request.StartDate >= request.EndDate)
            {
                return new BadRequestObjectResult("Start date must be before end date.");
            }

            if (request.StartDate < DateTime.UtcNow)
            {
                return new BadRequestObjectResult("Start date cannot be in the past.");
            }

            // Remove overlapping announcements
            var existingAnnouncements = await _context.BannerAnnouncements
                .Where(ba => ba.StartDate <= request.EndDate && ba.EndDate >= request.StartDate)
                .ToListAsync();

            if (existingAnnouncements.Any())
            {
                _context.BannerAnnouncements.RemoveRange(existingAnnouncements);
            }

            // Create new banner announcement
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

            // Return response
            return new CreateBannerAnnouncementResponse
            {
                Id = bannerAnnouncement.Id,
                Heading = bannerAnnouncement.Heading,
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
}