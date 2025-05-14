using Backend.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class LoyaltyDiscountService : ILoyaltyDiscountService
{
    private readonly ApplicationDBContext _context;
    private const decimal LOYALTY_DISCOUNT_PERCENTAGE = 10.0m; // 10% discount
    private const int REQUIRED_ORDERS_FOR_LOYALTY_DISCOUNT = 11;

    public LoyaltyDiscountService(ApplicationDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the number of completed orders for a user
    /// </summary>
    public async Task<int> GetCompletedOrdersCount(Guid userId)
    {
        var totalOrders = await _context.Orders
            .CountAsync(o => o.UserId == userId && 
                       (o.Status == "Verified" || o.Status == "Completed"));
        
        // Apply modulo operation to get value within range of 1-10
        // When count is 10, 20, 30, etc., this returns 10
        // Otherwise returns count % 10 (1-9)
        return totalOrders % REQUIRED_ORDERS_FOR_LOYALTY_DISCOUNT == 0 && totalOrders > 0
            ? REQUIRED_ORDERS_FOR_LOYALTY_DISCOUNT 
            : totalOrders % REQUIRED_ORDERS_FOR_LOYALTY_DISCOUNT;
    }

    /// <summary>
    /// Checks if a user qualifies for a loyalty discount (exactly 10 completed orders)
    /// </summary>
    public async Task<bool> QualifiesForLoyaltyDiscount(Guid userId)
    {
        var completedOrdersCount = await GetCompletedOrdersCount(userId);
        // Discount applies only when count is exactly 10
        return completedOrdersCount == REQUIRED_ORDERS_FOR_LOYALTY_DISCOUNT;
    }

    /// <summary>
    /// Gets the loyalty discount percentage for a user
    /// </summary>
    public async Task<decimal> GetLoyaltyDiscountPercentage(Guid userId)
    {
        if (await QualifiesForLoyaltyDiscount(userId))
        {
            return LOYALTY_DISCOUNT_PERCENTAGE;
        }
        
        return 0;
    }
} 