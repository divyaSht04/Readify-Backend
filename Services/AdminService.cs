using Backend.Context;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Backend.enums;
using Backend.Utils;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class AdminService
    {
        private readonly ApplicationDBContext _context;
        private readonly JwtUtils _jwtUtils;
        private readonly IConfiguration _configuration;

        public AdminService(ApplicationDBContext context, JwtUtils jwtUtils, IConfiguration configuration)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _configuration = configuration;
        }

        public async Task<ActionResult<GlobalDiscountResponse>> SetGlobalDiscount(CreateGlobalDiscountRequest request)
        {
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

        public async Task<ActionResult<StaffResponse>> AddStaff(AddStaffRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return new ConflictObjectResult("A user with this email already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var staff = new Users
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Password = hashedPassword,
                Role = Roles.STAFF,
                Created = DateTime.UtcNow,
            };

            _context.Users.Add(staff);
            await _context.SaveChangesAsync();

            var authResponse = await GenerateAuthResponse(staff);

            return new StaffResponse
            {
                Id = staff.Id.ToString(),
                Email = staff.Email,
                Role = staff.Role.ToString(),
                CreatedAt = staff.Created,
                RefreshToken = authResponse?.RefreshToken
            };
        }

        private async Task<AuthResponse> GenerateAuthResponse(Users user)
        {
            var token = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["JWT:RefreshTokenValidityInDays"] ?? "7"));

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["JWT:TokenValidityInMinutes"] ?? "60")),
                UserId = user.Id.ToString(),
                Email = user.Email,
                Role = user.Role.ToString(),
                Name = user.Name
            };
        }
    }
}