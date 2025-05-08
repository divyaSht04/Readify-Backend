using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Services;

public interface IUserService
{
    Task<ActionResult> EditProfile(string userId, EditProfileRequest request);
}