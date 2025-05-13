using Backend.Dtos.BookReview;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/reviews")]
public class BookReviewController : ControllerBase
{
    private readonly IBookReviewService _reviewService;

    public BookReviewController(IBookReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<BookReviewResponse>>> GetBookReviews(Guid bookId)
    {
        return await _reviewService.GetBookReviews(bookId);
    }
    
    [HttpGet("book/{bookId}/user")]
    [Authorize]
    public async Task<ActionResult<BookReviewResponse>> GetUserReview(Guid bookId)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user credentials");
        }
        
        return await _reviewService.GetUserReview(bookId, userId);
    }
    
    [HttpGet("book/{bookId}/can-review")]
    [Authorize]
    public async Task<ActionResult<bool>> CanUserReviewBook(Guid bookId)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user credentials");
        }
        
        return await _reviewService.CanUserReviewBook(userId, bookId);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookReviewResponse>> CreateReview(BookReviewRequest request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user credentials");
        }
        
        return await _reviewService.CreateReview(userId, request);
    }
    
    [HttpPut("{reviewId}")]
    [Authorize]
    public async Task<ActionResult<BookReviewResponse>> UpdateReview(Guid reviewId, BookReviewRequest request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user credentials");
        }
        
        return await _reviewService.UpdateReview(reviewId, userId, request);
    }
    
    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(Guid reviewId)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid user credentials");
        }
        
        return await _reviewService.DeleteReview(reviewId, userId);
    }
} 