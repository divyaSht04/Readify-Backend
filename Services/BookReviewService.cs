using Backend.Context;
using Backend.Dtos.BookReview;
using Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BookReviewService : IBookReviewService
{
    private readonly ApplicationDBContext _context;

    public BookReviewService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<ActionResult<IEnumerable<BookReviewResponse>>> GetBookReviews(Guid bookId)
    {
        var reviews = await _context.BookReviews
            .Include(br => br.User)
            .Where(br => br.BookId == bookId)
            .OrderByDescending(br => br.CreatedAt)
            .ToListAsync();

        var reviewResponses = reviews.Select(r => new BookReviewResponse
        {
            Id = r.Id,
            BookId = r.BookId,
            UserId = r.UserId,
            UserName = r.User.Name,
            UserImage = r.User.Image ?? "",
            Rating = r.Rating,
            ReviewText = r.ReviewText ?? "",
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });

        return reviewResponses.ToList();
    }

    public async Task<ActionResult<BookReviewResponse>> GetUserReview(Guid bookId, Guid userId)
    {
        var review = await _context.BookReviews
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.BookId == bookId && br.UserId == userId);

        if (review == null)
        {
            return new NotFoundObjectResult("Review not found");
        }

        var response = new BookReviewResponse
        {
            Id = review.Id,
            BookId = review.BookId,
            UserId = review.UserId,
            UserName = review.User.Name,
            UserImage = review.User.Image ?? "",
            Rating = review.Rating,
            ReviewText = review.ReviewText ?? "",
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };

        return response;
    }

    public async Task<ActionResult<BookReviewResponse>> CreateReview(Guid userId, BookReviewRequest request)
    {
        // Check if the user can review this book (has a verified order with this book)
        var canReview = await CanUserReviewBook(userId, request.BookId);
        if (canReview.Result is OkObjectResult okResult)
        {
            if (!(bool)okResult.Value)
            {
                return new BadRequestObjectResult("You can only review books that you have purchased with verified orders.");
            }
        }
        else
        {
            return new BadRequestObjectResult("Could not verify user's eligibility to review this book.");
        }

        // Check if the user has already reviewed this book
        var existingReview = await _context.BookReviews
            .FirstOrDefaultAsync(br => br.BookId == request.BookId && br.UserId == userId);

        if (existingReview != null)
        {
            return new BadRequestObjectResult("You have already reviewed this book.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new NotFoundObjectResult("User not found");
        }

        var book = await _context.Books.FindAsync(request.BookId);
        if (book == null)
        {
            return new NotFoundObjectResult("Book not found");
        }

        var review = new BookReview
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            UserId = userId,
            Rating = request.Rating,
            ReviewText = request.ReviewText,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.BookReviews.AddAsync(review);
        await _context.SaveChangesAsync();

        var response = new BookReviewResponse
        {
            Id = review.Id,
            BookId = review.BookId,
            UserId = review.UserId,
            UserName = user.Name,
            UserImage = user.Image ?? "",
            Rating = review.Rating,
            ReviewText = review.ReviewText ?? "",
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };

        return response;
    }

    public async Task<ActionResult<BookReviewResponse>> UpdateReview(Guid reviewId, Guid userId, BookReviewRequest request)
    {
        var review = await _context.BookReviews
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.Id == reviewId && br.UserId == userId);

        if (review == null)
        {
            return new NotFoundObjectResult("Review not found or you don't have permission to update it");
        }

        // Verify the book ID in the request matches the review's book ID
        if (review.BookId != request.BookId)
        {
            return new BadRequestObjectResult("Book ID in the request does not match the review's book ID");
        }

        review.Rating = request.Rating;
        review.ReviewText = request.ReviewText;
        review.UpdatedAt = DateTime.UtcNow;

        _context.BookReviews.Update(review);
        await _context.SaveChangesAsync();

        var response = new BookReviewResponse
        {
            Id = review.Id,
            BookId = review.BookId,
            UserId = review.UserId,
            UserName = review.User.Name,
            UserImage = review.User.Image ?? "",
            Rating = review.Rating,
            ReviewText = review.ReviewText ?? "",
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };

        return response;
    }

    public async Task<ActionResult> DeleteReview(Guid reviewId, Guid userId)
    {
        var review = await _context.BookReviews
            .FirstOrDefaultAsync(br => br.Id == reviewId && br.UserId == userId);

        if (review == null)
        {
            return new NotFoundObjectResult("Review not found or you don't have permission to delete it");
        }

        _context.BookReviews.Remove(review);
        await _context.SaveChangesAsync();

        return new OkResult();
    }

    public async Task<ActionResult<bool>> CanUserReviewBook(Guid userId, Guid bookId)
    {
        // User can review a book only if:
        // 1. The user has a verified order
        // 2. The order contains this book

        var hasVerifiedOrder = await _context.Orders
            .Include(o => o.Items)
            .AnyAsync(o => 
                o.UserId == userId && 
                o.Status == "Verified" && 
                o.Items.Any(item => item.BookId == bookId));

        return hasVerifiedOrder;
    }
} 