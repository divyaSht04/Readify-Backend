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

        [HttpPost("global-discount")]
        public async Task<ActionResult<GlobalDiscountResponse>> SetGlobalDiscount([FromBody] CreateGlobalDiscountRequest request)
        {
            return await _adminService.SetGlobalDiscount(request);
        }
        
        [HttpDelete("global-discount")]
        public async Task<ActionResult> RemoveGlobalDiscount()
        {
            return await _adminService.RemoveGlobalDiscount();
        }
        
        
    }
}