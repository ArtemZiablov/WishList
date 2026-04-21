namespace WishListApp.DTOs;

public class UserDtos
{
    public record UserResponse(
        Guid Id,
        string Email,
        string DisplayName,
        string? AvatarUrl,
        DateTime CreatedAt
    );

    public record UpdateUserRequest(
        string DisplayName,
        string? AvatarUrl
    );
}