using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface ILoyaltyDiscountService
{
    /// <summary>
    /// Gets the number of completed orders for a user
    /// </summary>
    Task<int> GetCompletedOrdersCount(Guid userId);
    
    /// <summary>
    /// Checks if a user qualifies for a loyalty discount (10+ completed orders)
    /// </summary>
    Task<bool> QualifiesForLoyaltyDiscount(Guid userId);
    
    /// <summary>
    /// Gets the loyalty discount percentage for a user
    /// </summary>
    Task<decimal> GetLoyaltyDiscountPercentage(Guid userId);
} 