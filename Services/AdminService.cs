using Backend.Context;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class AdminService
    {
        private readonly ApplicationDBContext _context;

        public AdminService(ApplicationDBContext context)
        {
            _context = context;
        }
        

        public async Task<ActionResult<GlobalDiscountResponse>> SetGlobalDiscount(CreateGlobalDiscountRequest request)
        {
            // Optional: Remove any existing active or overlapping discounts to avoid conflicts
            var existingDiscounts = await _context.GlobalDiscounts
                .Where(gd => gd.StartDate <= request.EndDate && gd.EndDate >= request.StartDate)
                .ToListAsync();

            if (existingDiscounts.Any())
            {
                _context.GlobalDiscounts.RemoveRange(existingDiscounts);
            }

            var globalDiscount = new GlobalDiscount
            {
                Id = Guid.NewGuid(),
                DiscountName = request.DiscountName,
                Percentage = request.Percentage,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OnSale = request.OnSale,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GlobalDiscounts.Add(globalDiscount);
            await _context.SaveChangesAsync();

            return new GlobalDiscountResponse
            {
                Id = globalDiscount.Id,
                DiscountName = globalDiscount.DiscountName,
                Percentage = globalDiscount.Percentage,
                StartDate = globalDiscount.StartDate,
                EndDate = globalDiscount.EndDate,
                OnSale = globalDiscount.OnSale,
                CreatedAt = globalDiscount.CreatedAt,
                UpdatedAt = globalDiscount.UpdatedAt
            };
        }
        
        public async Task<ActionResult> RemoveGlobalDiscount()
        {
            var activeDiscounts = await _context.GlobalDiscounts
                .Where(gd => gd.StartDate <= DateTime.UtcNow && gd.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            if (!activeDiscounts.Any())
            {
                return new NotFoundObjectResult("No active global discount found.");
            }

            _context.GlobalDiscounts.RemoveRange(activeDiscounts);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Global discount removed successfully" });
        }
    }
}