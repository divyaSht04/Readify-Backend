using Backend.Context;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Backend.Dtos.Bannner;
using Backend.enums;
using Backend.Utils;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class AdminService
    {
        private readonly ApplicationDBContext _context;

        public AdminService(ApplicationDBContext context)
        {
            _context = context;
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
                IsVerified = true,

                IsActive = true,

            };

            _context.Users.Add(staff);
            await _context.SaveChangesAsync();

            return new StaffResponse
            {
                Id = staff.Id.ToString(),
                Email = staff.Email,
                Role = staff.Role.ToString(),
                CreatedAt = staff.Created,
            };
        }
        public async Task<ActionResult<StaffResponse>> GetStaff(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                return new BadRequestObjectResult("Staff ID is required.");
            }

            if (!Guid.TryParse(staffId, out Guid id))
            {
                return new BadRequestObjectResult("Invalid staff ID format.");
            }

            var staff = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Roles.STAFF);

            if (staff == null)
            {
                return new NotFoundObjectResult($"Staff with ID {staffId} not found.");
            }

            return new StaffResponse
            {
                Id = staff.Id.ToString(),
                Email = staff.Email,
                Role = staff.Role.ToString(),
                CreatedAt = staff.Created,
            };
        }

        public async Task<ActionResult> DeactivateStaff(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                return new BadRequestObjectResult("Staff ID is required.");
            }

            if (!Guid.TryParse(staffId, out Guid id))
            {
                return new BadRequestObjectResult("Invalid staff ID format.");
            }

            var staff = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Roles.STAFF);

            if (staff == null)
            {
                return new NotFoundObjectResult($"Staff with ID {staffId} not found.");
            }

            if (!staff.IsActive)
            {
                return new BadRequestObjectResult($"Staff with ID {staffId} is already deactivated.");
            }

            staff.IsActive = false;
            
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = $"Staff with ID {staffId} deactivated successfully" });
        }

        public async Task<ActionResult> ReactivateStaff(string staffId)
        {
            if (string.IsNullOrEmpty(staffId))
            {
                return new BadRequestObjectResult("Staff ID is required.");
            }

            if (!Guid.TryParse(staffId, out Guid id))
            {
                return new BadRequestObjectResult("Invalid staff ID format.");
            }

            var staff = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Roles.STAFF);

            if (staff == null)
            {
                return new NotFoundObjectResult($"Staff with ID {staffId} not found.");
            }

            if (staff.IsActive)
            {
                return new BadRequestObjectResult($"Staff with ID {staffId} is already active.");
            }

            staff.IsActive = true;
            
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = $"Staff with ID {staffId} reactivated successfully" });
        }
    }
    
}