using Backend.Dtos.BookReview;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IBookReviewService
{
    Task<ActionResult<IEnumerable<BookReviewResponse>>> GetBookReviews(Guid bookId);
    Task<ActionResult<BookReviewResponse>> GetUserReview(Guid bookId, Guid userId);
    Task<ActionResult<BookReviewResponse>> CreateReview(Guid userId, BookReviewRequest request);
    Task<ActionResult<BookReviewResponse>> UpdateReview(Guid reviewId, Guid userId, BookReviewRequest request);
    Task<ActionResult> DeleteReview(Guid reviewId, Guid userId);
    Task<ActionResult<bool>> CanUserReviewBook(Guid userId, Guid bookId);
} 