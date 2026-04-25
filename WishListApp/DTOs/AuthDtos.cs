namespace WishListApp.DTOs;

public class AuthDtos
{
    public record RegisterRequest(
        string Email,
        string Password,
        string DisplayName
    );

    public record LoginRequest(
        string Email,
        string Password
    );

    public record AuthResponse(
        string Token,
        DateTime ExpiresAt,
        Guid UserId,
        string Email,
        string DisplayName
    );
}