using Backend.Context;
using Backend.Dtos.Booking;
using Backend.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDBContext _context;

    public BookingService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .ToListAsync();
            
        return bookings.Select(MapToBookingResponse).ToList();
    }

    public async Task<ActionResult<List<BookingResponse>>> GetBookingsByUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new NotFoundObjectResult($"User with ID {userId} not found.");
        }

        var bookings = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .Where(b => b.UserID == userId)
            .ToListAsync();
            
        return bookings.Select(MapToBookingResponse).ToList();
    }

    public async Task<ActionResult<BookingResponse>> GetBookingById(Guid id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.ID == id);
            
        if (booking == null)
        {
            return new NotFoundObjectResult($"Booking with ID {id} not found.");
        }
        
        return MapToBookingResponse(booking);
    }

    public async Task<ActionResult<BookingResponse>> CreateBooking(Guid userId, CreateBookingRequest request)
    {
        // Check if user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new NotFoundObjectResult($"User with ID {userId} not found.");
        }
        
        // Check if book exists
        var book = await _context.Books.FindAsync(request.BookID);
        if (book == null)
        {
            return new NotFoundObjectResult($"Book with ID {request.BookID} not found.");
        }
        
        // Check if there's enough stock
        if (book.StockQuantity < request.Quantity && !book.IsComingSoon)
        {
            return new BadRequestObjectResult($"Not enough stock available. Current stock: {book.StockQuantity}");
        }
        
        var totalPrice = book.Price * request.Quantity;
        
        // Create booking
        var booking = new Booking
        {
            ID = Guid.NewGuid(),
            UserID = userId,
            BookID = request.BookID,
            Quantity = request.Quantity,
            Status = BookingStatus.Pending,
            TotalPrice = totalPrice,
            BookingDate = DateTime.UtcNow,
        };
        
        if (!book.IsComingSoon)
        {
            book.StockQuantity -= request.Quantity;
        }
        
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
        
        booking = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.ID == booking.ID);
            
        return MapToBookingResponse(booking);
    }

    public async Task<ActionResult<BookingResponse>> UpdateBooking(Guid id, UpdateBookingRequest request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.ID == id);
            
        if (booking == null)
        {
            return new NotFoundObjectResult($"Booking with ID {id} not found.");
        }
        
        // Handle quantity change
        if (request.Quantity.HasValue && request.Quantity.Value != booking.Quantity)
        {
            var book = booking.Book;
            
            // If it's not a coming soon book, check and update stock
            if (!book.IsComingSoon)
            {
                // Return stock from the original booking
                book.StockQuantity += booking.Quantity;
                
                // Check if there's enough stock for the new quantity
                if (book.StockQuantity < request.Quantity.Value)
                {
                    // Revert stock change
                    book.StockQuantity -= booking.Quantity;
                    return new BadRequestObjectResult($"Not enough stock available. Current stock: {book.StockQuantity}");
                }
                
                // Update stock with new quantity
                book.StockQuantity -= request.Quantity.Value;
            }
            
            // Update booking quantity and total price
            booking.Quantity = request.Quantity.Value;
            booking.TotalPrice = book.Price * request.Quantity.Value;
        }
        
        // Update status if provided
        if (request.Status.HasValue)
        {
            booking.Status = request.Status.Value;
        }
        
        
        await _context.SaveChangesAsync();
        
        return MapToBookingResponse(booking);
    }

    public async Task<ActionResult> CancelBooking(Guid id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Book)
            .FirstOrDefaultAsync(b => b.ID == id);
            
        if (booking == null)
        {
            return new NotFoundObjectResult($"Booking with ID {id} not found.");
        }
        
        // Only allow cancellation of pending or confirmed bookings
        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
        {
            return new BadRequestObjectResult($"Cannot cancel a booking with status {booking.Status}");
        }
        
        // Update booking status
        booking.Status = BookingStatus.Cancelled;
        
        // If it's not a coming soon book, return the stock
        if (!booking.Book.IsComingSoon)
        {
            booking.Book.StockQuantity += booking.Quantity;
        }
        
        await _context.SaveChangesAsync();
        
        return new OkResult();
    }

    public async Task<ActionResult<List<BookingResponse>>> GetBookingsByStatus(BookingStatus status)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Book)
            .Include(b => b.User)
            .Where(b => b.Status == status)
            .ToListAsync();
            
        return bookings.Select(MapToBookingResponse).ToList();
    }
    
    // Helper method to map from Booking entity to BookingResponse DTO
    private static BookingResponse MapToBookingResponse(Booking booking)
    {
        return new BookingResponse
        {
            ID = booking.ID,
            UserID = booking.UserID,
            UserName = booking.User?.Name,
            BookID = booking.BookID,
            BookTitle = booking.Book?.Title,
            BookAuthor = booking.Book?.Author,
            Quantity = booking.Quantity,
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
        };
    }
}
