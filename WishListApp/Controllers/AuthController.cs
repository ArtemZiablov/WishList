using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WishListApp.DTOs;
using WishListApp.Models;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthDtos.AuthResponse>> Register(AuthDtos.RegisterRequest request)
    {
        // Check if email is already taken
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Conflict("A user with this email already exists.");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
        };

        // UserManager hashes the password and saves the user
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // result.Errors contains things like "Password too short"
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(errors);
        }

        var token = GenerateJwtToken(user);
        return Ok(BuildAuthResponse(user, token));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthDtos.AuthResponse>> Login(AuthDtos.LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Invalid email or password.");

        // CheckPasswordSignInAsync verifies the hash — lockoutOnFailure tracks failed attempts
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return StatusCode(429, "Account is locked due to too many failed attempts. Try again later.");

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var token = GenerateJwtToken(user);
        return Ok(BuildAuthResponse(user, token));
    }

    // Private helpers
    private string GenerateJwtToken(User user)
    {
        // Claims are key-value pairs embedded inside the token
        // These are what your controllers read via User.FindFirstValue(...)
        var claims = new[]
        {
            // NameIdentifier = the userId — this is what GetCurrentUserId() reads
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.DisplayName),
            // JTI = unique ID for this token — useful for token revocation later
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddDays(7);  // token valid for 7 days

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthDtos.AuthResponse BuildAuthResponse(User user, string token)
    {
        var expiry = DateTime.UtcNow.AddDays(7);
        return new AuthDtos.AuthResponse(token, expiry, user.Id, user.Email!, user.DisplayName);
    }
}