using WishListApp.DTOs;
using WishListApp.Models;

namespace WishListApp.Interfaces;


public interface IWishListAccessService
{
    // Called when someone opens the public link — no auth required
    Task<WishListDtos.WishListResponse?> GetByPublicTokenAsync(string token);

    // Called when someone opens the invite link — must be logged in
    // Returns info about the list so they can decide whether to request access
    Task<WishListAccessDtos.InviteLinkInfoResponse?> GetInviteLinkInfoAsync(string inviteToken);

    // Logged-in user requests access via invite link
    Task<bool> RequestAccessAsync(string inviteToken, Guid requestingUserId);

    // Owner approves or rejects a pending request
    Task<bool> RespondToRequestAsync(Guid requestId, Guid ownerId, AccessRequestStatus response);

    // Owner sees all pending requests for their lists
    Task<List<WishListAccessDtos.AccessRequestResponse>> GetPendingRequestsAsync(Guid ownerId);

    // Central access check used by WishListService
    Task<bool> CanUserAccessAsync(Guid wishListId, Guid userId);
}