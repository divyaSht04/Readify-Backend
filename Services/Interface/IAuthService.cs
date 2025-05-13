using System.Threading.Tasks;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IAuthService
{
    Task<ActionResult<AuthResponse>> Login(LoginRequest request);
    Task<ActionResult> Register(RegisterRequest request);
    Task<ActionResult<AuthResponse>> VerifyOtp(VerifyOtpRequest request);
    Task<ActionResult> ResendOtp(string email);
    Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request);
    Task<ActionResult> RevokeToken(string userId);
    Task<ActionResult> ChangePassword(string userId, ChangePasswordRequest request);
}