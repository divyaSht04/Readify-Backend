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

    }
}