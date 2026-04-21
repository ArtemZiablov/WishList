using WishListApp.Models;

namespace WishListApp.DTOs;

public class FriendshipDtos
{
    public record FriendshipResponse(
        Guid Id,
        Guid RequesterId,
        string RequesterName,
        string? RequesterAvatarUrl,
        FriendshipStatus Status,
        DateTime CreatedAt
    );
}