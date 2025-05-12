using Backend.Dtos.Booking;
using Backend.enums;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IBookingService
{
    Task<ActionResult<List<BookingResponse>>> GetAllBookings();
    Task<ActionResult<List<BookingResponse>>> GetBookingsByUser(Guid userId);
    Task<ActionResult<BookingResponse>> GetBookingById(Guid id);
    Task<ActionResult<BookingResponse>> CreateBooking(Guid userId, CreateBookingRequest request);
    Task<ActionResult<BookingResponse>> UpdateBooking(Guid id, UpdateBookingRequest request);
    Task<ActionResult> CancelBooking(Guid id);
    Task<ActionResult<List<BookingResponse>>> GetBookingsByStatus(BookingStatus status);
}
