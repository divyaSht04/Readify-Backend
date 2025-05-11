using Backend.Dtos.Booking;
using Backend.enums;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("/booking")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
    {
        return await _bookingService.GetAllBookings();
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<BookingResponse>>> GetMyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("Invalid user ID");
        }

        return await _bookingService.GetBookingsByUser(userGuid);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingResponse>> GetBookingById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("Invalid user ID");
        }

        var result = await _bookingService.GetBookingById(id);
        
        // If the result is not found, return the not found result
        if (result.Result is NotFoundObjectResult)
        {
            return result;
        }
        
        // Only allow users to view their own bookings unless they are admins
        if (result.Value.UserID != userGuid && !User.IsInRole("ADMIN"))
        {
            return Forbid("You do not have permission to view this booking");
        }
        
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking(CreateBookingRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("Invalid user ID");
        }

        return await _bookingService.CreateBooking(userGuid, request);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BookingResponse>> UpdateBooking(Guid id, UpdateBookingRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("Invalid user ID");
        }

        // Get the booking to check ownership
        var bookingResult = await _bookingService.GetBookingById(id);
        
        // If the booking is not found, return the not found result
        if (bookingResult.Result is NotFoundObjectResult)
        {
            return bookingResult;
        }
        
        // Only allow users to update their own bookings unless they are admins
        if (bookingResult.Value.UserID != userGuid && !User.IsInRole("ADMIN"))
        {
            return Forbid("You do not have permission to update this booking");
        }
        
        // Regular users can only update the pickup date
        if (!User.IsInRole("ADMIN") && (request.Status.HasValue || request.Quantity.HasValue))
        {
            return BadRequest("Regular users can only update the pickup date");
        }
        
        return await _bookingService.UpdateBooking(id, request);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelBooking(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized("Invalid user ID");
        }

        // Get the booking to check ownership
        var bookingResult = await _bookingService.GetBookingById(id);
        
        // If the booking is not found, return the not found result
        if (bookingResult.Result is NotFoundObjectResult)
        {
            return bookingResult.Result;
        }
        
        // Only allow users to cancel their own bookings unless they are admins
        if (bookingResult.Value.UserID != userGuid && !User.IsInRole("ADMIN"))
        {
            return Forbid("You do not have permission to cancel this booking");
        }
        
        return await _bookingService.CancelBooking(id);
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<BookingResponse>>> GetBookingsByStatus(BookingStatus status)
    {
        return await _bookingService.GetBookingsByStatus(status);
    }
}
