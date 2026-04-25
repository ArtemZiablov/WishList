using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WishListApp.DTOs;
using WishListApp.Interfaces;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/users/me
    [HttpGet("me")]
    public async Task<ActionResult<UserDtos.UserResponse>> GetMe()
    {
        var user = await _userService.GetByIdAsync(GetCurrentUserId());
        return user is null ? NotFound() : Ok(user);
    }

    // GET /api/users/search?email=test@gmail.com
    [HttpGet("search")]
    public async Task<ActionResult<List<UserDtos.UserResponse>>> Search([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email query parameter is required.");

        var users = await _userService.SearchByEmailAsync(email);
        return Ok(users);
    }

    // PUT /api/users/me
    [HttpPut("me")]
    public async Task<ActionResult<UserDtos.UserResponse>> UpdateProfile(UserDtos.UpdateUserRequest request)
    {
        var result = await _userService.UpdateProfileAsync(GetCurrentUserId(), request);
        return result is null ? NotFound() : Ok(result);
    }
}
