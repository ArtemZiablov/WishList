using WishListApp.Models;

namespace WishListApp.DTOs;

public class WishListDtos
{

    // What the client sends when CREATING
    public record CreateWishListRequest(
        string Title,
        string? Description,
        EventType EventType,
        DateTime? EventDate,
        WishListVisibility Visibility
    );

    // What the client sends when UPDATING
    public record UpdateWishListRequest(
        string Title,
        string? Description,
        EventType EventType,
        DateTime? EventDate,
        WishListVisibility Visibility
    );

    // What the API sends BACK to the client
    public record WishListResponse(
        Guid Id,
        string? Title,
        string? Description,
        EventType EventType,
        DateTime? EventDate,
        string ShareToken,
        string? InviteToken,
        int ItemCount,
        WishListVisibility Visibility
    );
}