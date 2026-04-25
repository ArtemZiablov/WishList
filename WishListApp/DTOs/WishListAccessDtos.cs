using WishListApp.Models;

namespace WishListApp.DTOs;

public class WishListAccessDtos
{
    // What the requester sees after clicking an invite link
    public record InviteLinkInfoResponse(
        Guid WishListId,
        string WishListTitle,
        string OwnerName
    );

    // What the owner sees in their pending requests list
    public record AccessRequestResponse(
        Guid Id,
        Guid WishListId,
        string? WishListTitle,
        Guid RequestedByUserId,
        string RequesterName,
        string RequesterEmail,
        AccessRequestStatus Status,
        DateTime RequestedAt
    );
}