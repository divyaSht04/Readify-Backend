using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("/admin")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("staff")]
        public async Task<ActionResult<StaffResponse>> AddStaff([FromBody] AddStaffRequest request)
        {
            return await _adminService.AddStaff(request);
        }
        [HttpGet("staff/{staffId}")]
        public async Task<ActionResult<StaffResponse>> GetStaff(string staffId)
        {
            return await _adminService.GetStaff(staffId);
        }

        [HttpPut("staff/{staffId}/deactivate")]
        public async Task<ActionResult> DeactivateStaff(string staffId)
        {
            return await _adminService.DeactivateStaff(staffId);
        }

        [HttpPut("staff/{staffId}/reactivate")]
        public async Task<ActionResult> ReactivateStaff(string staffId)
        {
            return await _adminService.ReactivateStaff(staffId);
        }

    }
}