using System.Threading.Tasks;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IAuthService
{
    Task<ActionResult<AuthResponse>> Login(LoginRequest request);
    Task<ActionResult<AuthResponse>> Register(RegisterRequest request);
    Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request);
    Task<ActionResult> RevokeToken(string userId);
}