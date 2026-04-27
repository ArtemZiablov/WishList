using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class AuthTokenProcessor : IAuthTokenProcessor
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenProcessor(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Short-lived access token — 15 minutes is standard for production.
        // The refresh token handles keeping the user logged in long-term.
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public string GenerateRefreshToken()
    {
        // Cryptographically random — 64 bytes gives 512 bits of entropy,
        // making it effectively impossible to guess
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiresAtUtc)
    {
        // HttpOnly = JavaScript cannot access this cookie at all.
        // This protects against XSS attacks stealing the token.
        // Secure = only sent over HTTPS (set false for localhost development).
        // SameSite = Strict prevents the cookie being sent on cross-site requests (CSRF protection).
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(cookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,      // set to false only if testing over HTTP
            SameSite = SameSiteMode.None, // None needed for cross-origin (React on different port)
            Expires = expiresAtUtc
        });
    }
}