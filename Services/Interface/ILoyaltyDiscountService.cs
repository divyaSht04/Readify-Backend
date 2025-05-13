using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface ILoyaltyDiscountService
{
    Task<int> GetCompletedOrdersCount(Guid userId);
    
    Task<bool> QualifiesForLoyaltyDiscount(Guid userId);
    
    Task<decimal> GetLoyaltyDiscountPercentage(Guid userId);
} 