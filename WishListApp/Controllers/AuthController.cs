// Controllers/AuthController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IAuthTokenProcessor _tokenProcessor;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IAuthTokenProcessor tokenProcessor,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenProcessor = tokenProcessor;
        _configuration = configuration;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthDtos.AuthResponse>> Register(AuthDtos.RegisterRequest request)
    {
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

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // Issue tokens immediately after registration — no need to log in separately
        await IssueTokensToUser(user);

        return Ok(new AuthDtos.AuthResponse(user.Id, user.Email!, user.DisplayName, user.AvatarUrl));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthDtos.AuthResponse>> Login(AuthDtos.LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return StatusCode(429, "Account locked. Try again later.");

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        await IssueTokensToUser(user);

        return Ok(new AuthDtos.AuthResponse(user.Id, user.Email!, user.DisplayName, user.AvatarUrl));
    }

    // POST /api/auth/refresh
    // Called automatically by the frontend when the access token expires.
    // Reads the refresh token from the HttpOnly cookie — the frontend doesn't touch it.
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthDtos.AuthResponse>> Refresh()
    {
        var refreshToken = Request.Cookies["REFRESH_TOKEN"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token is missing.");

        // Find the user who owns this refresh token
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user is null)
            return Unauthorized("Invalid refresh token.");

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized("Refresh token has expired. Please log in again.");

        // Issue a completely new pair of tokens — the old refresh token is replaced
        await IssueTokensToUser(user);

        return Ok(new AuthDtos.AuthResponse(user.Id, user.Email!, user.DisplayName, user.AvatarUrl));
    }

    // POST /api/auth/logout
    // Clears the cookies and invalidates the refresh token in the DB.
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["REFRESH_TOKEN"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user is not null)
            {
                // Invalidate the refresh token so it can't be reused after logout
                user.RefreshToken = null;
                user.RefreshTokenExpiresAtUtc = null;
                await _userManager.UpdateAsync(user);
            }
        }

        // Delete the cookies by setting them to expire immediately
        Response.Cookies.Delete("ACCESS_TOKEN");
        Response.Cookies.Delete("REFRESH_TOKEN");

        return NoContent();
    }

    // GET /api/auth/google
    // Step 1: redirect the browser to Google's login page.
    // Uses SignInManager which automatically adds CSRF correlation tokens for security.
    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
    {
        // ConfigureExternalAuthenticationProperties builds the correct Google redirect URL
        // and embeds a CSRF token to verify the callback is legitimate
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(
            "Google",
            Url.Action(nameof(GoogleCallback), new { returnUrl }));

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // GET /api/auth/google/callback
    // Step 2: Google redirects here. By this point ASP.NET has already exchanged
    // the authorization code for user info — we just read what Google told us.
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl = "/")
    {
        // Read the result that the Google middleware already prepared
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return Redirect(BuildFrontendErrorUrl(returnUrl, "google_auth_failed"));

        var claimsPrincipal = result.Principal;
        var email = claimsPrincipal?.FindFirstValue(ClaimTypes.Email);

        if (email is null)
            return Redirect(BuildFrontendErrorUrl(returnUrl, "missing_email"));

        // Try to find existing user — this implements Option 1 (auto-merge)
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            // First time logging in with Google — create a new account.
            // No password because they'll always authenticate via Google.
            user = new User
            {
                UserName = email,
                Email = email,
                DisplayName = claimsPrincipal?.FindFirstValue(ClaimTypes.Name)
                              ?? email.Split('@')[0],
                AvatarUrl = claimsPrincipal?.Claims
                    .FirstOrDefault(c => c.Type.Contains("picture"))?.Value,
                EmailConfirmed = true, // Google already verified this
                CreatedAt = DateTime.UtcNow,
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
                return Redirect(BuildFrontendErrorUrl(returnUrl, "account_creation_failed"));
        }
        else if (user.AvatarUrl is null)
        {
            // Backfill avatar for existing users who didn't have one
            var pictureUrl = claimsPrincipal?.Claims
                .FirstOrDefault(c => c.Type.Contains("picture"))?.Value;

            if (pictureUrl is not null)
            {
                user.AvatarUrl = pictureUrl;
                await _userManager.UpdateAsync(user);
            }
        }

        // Link this Google account to the user in the AspNetUserLogins table.
        // This is the correct Identity way to track external login providers.
        // It stores the Google ID so future logins can be matched by Google ID,
        // not just email — more reliable since emails can change.
        var googleId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var loginInfo = new UserLoginInfo("Google", googleId, "Google");

        // AddLoginAsync is idempotent — if the link already exists it returns success
        var existingLogin = await _userManager.FindByLoginAsync("Google", googleId);
        if (existingLogin is null)
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
                return Redirect(BuildFrontendErrorUrl(returnUrl, "login_link_failed"));
        }

        // Issue tokens as HttpOnly cookies — same as email/password login
        await IssueTokensToUser(user);

        // Redirect to React. Tokens are in cookies, not the URL.
        var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        return Redirect($"{frontendBase}{returnUrl}");
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    // Generates both tokens and writes them as HttpOnly cookies.
    // Called from Register, Login, Refresh, and GoogleCallback — all in one place.
    private async Task IssueTokensToUser(User user)
    {
        var (jwtToken, jwtExpiresAt) = _tokenProcessor.GenerateJwtToken(user);
        var refreshToken = _tokenProcessor.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);

        // Persist the refresh token — we need to look it up later in /refresh
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAtUtc = refreshExpiresAt;
        await _userManager.UpdateAsync(user);

        // Write both tokens as HttpOnly cookies — browser handles them automatically
        _tokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, jwtExpiresAt);
        _tokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshToken, refreshExpiresAt);
    }

    private string BuildFrontendErrorUrl(string returnUrl, string error)
    {
        var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        return $"{frontendBase}/auth/error?error={error}&returnUrl={returnUrl}";
    }
}