using System.Security.Claims;

namespace Backend.Utils;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
            return Guid.Empty;
        
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || string.IsNullOrEmpty(claim.Value))
            return Guid.Empty;
        
        if (Guid.TryParse(claim.Value, out Guid userId))
            return userId;
        
        return Guid.Empty;
    }
} 