using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;


public class WishListAccessService : IWishListAccessService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public WishListAccessService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<WishListDtos.WishListResponse?> GetByPublicTokenAsync(string token)
    {
        var wishList = await _db.WishLists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w =>
                w.ShareToken == token &&
                w.Visibility == WishListVisibility.Public);

        return wishList is null ? null : _mapper.Map<WishListDtos.WishListResponse>(wishList);
    }

    public async Task<WishListAccessDtos.InviteLinkInfoResponse?> GetInviteLinkInfoAsync(string inviteToken)
    {
        var wishList = await _db.WishLists
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w =>
                w.InviteToken == inviteToken &&
                w.Visibility == WishListVisibility.Invite);

        if (wishList is null) return null;

        return new WishListAccessDtos.InviteLinkInfoResponse(
            wishList.Id,
            wishList.Title ?? "Untitled",
            wishList.Owner.DisplayName
        );
    }

    public async Task<bool> RequestAccessAsync(string inviteToken, Guid requestingUserId)
    {
        var wishList = await _db.WishLists
            .FirstOrDefaultAsync(w =>
                w.InviteToken == inviteToken &&
                w.Visibility == WishListVisibility.Invite);

        if (wishList is null) return false;

        // Owner doesn't need to request access to their own list
        if (wishList.OwnerId == requestingUserId) return false;

        // Already approved — no need to request again
        var alreadyApproved = await _db.WishListAccessRequests.AnyAsync(r =>
            r.WishListId == wishList.Id &&
            r.RequestedByUserId == requestingUserId &&
            r.Status == AccessRequestStatus.Approved);

        if (alreadyApproved) return false;

        // Already has a pending request — don't create a duplicate
        var alreadyPending = await _db.WishListAccessRequests.AnyAsync(r =>
            r.WishListId == wishList.Id &&
            r.RequestedByUserId == requestingUserId &&
            r.Status == AccessRequestStatus.Pending);

        if (alreadyPending) return false;

        // Previous request was rejected — allow re-requesting
        var rejected = await _db.WishListAccessRequests
            .FirstOrDefaultAsync(r =>
                r.WishListId == wishList.Id &&
                r.RequestedByUserId == requestingUserId &&
                r.Status == AccessRequestStatus.Rejected);

        if (rejected is not null)
        {
            // Reset the rejected request to pending instead of creating a new row
            rejected.Status = AccessRequestStatus.Pending;
            rejected.RequestedAt = DateTime.UtcNow;
            rejected.RespondedAt = null;
        }
        else
        {
            _db.WishListAccessRequests.Add(new WishListAccessRequest
            {
                WishListId = wishList.Id,
                RequestedByUserId = requestingUserId,
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RespondToRequestAsync(Guid requestId, Guid ownerId, AccessRequestStatus response)
    {
        var request = await _db.WishListAccessRequests
            .Include(r => r.WishList)
            .FirstOrDefaultAsync(r =>
                r.Id == requestId &&
                r.WishList.OwnerId == ownerId);

        if (request is null) return false;
        if (request.Status != AccessRequestStatus.Pending) return false;
        if (response == AccessRequestStatus.Pending) return false;

        request.Status = response;
        request.RespondedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<WishListAccessDtos.AccessRequestResponse>> GetPendingRequestsAsync(Guid ownerId)
    {
        var requests = await _db.WishListAccessRequests
            .Include(r => r.WishList)
            .Include(r => r.RequestedBy)
            .Where(r =>
                r.WishList.OwnerId == ownerId &&
                r.Status == AccessRequestStatus.Pending)
            .ToListAsync();

        return requests.Select(r => new WishListAccessDtos.AccessRequestResponse(
            r.Id,
            r.WishListId,
            r.WishList.Title,
            r.RequestedByUserId,
            r.RequestedBy.DisplayName,
            r.RequestedBy.Email!,
            r.Status,
            r.RequestedAt
        )).ToList();
    }

    public async Task<bool> CanUserAccessAsync(Guid wishListId, Guid userId)
    {
        var wishList = await _db.WishLists
            .FirstOrDefaultAsync(w => w.Id == wishListId);

        if (wishList is null) return false;
        if (wishList.OwnerId == userId) return true;

        return wishList.Visibility switch
        {
            WishListVisibility.Public => true,

            WishListVisibility.Invite =>
                await _db.WishListAccessRequests.AnyAsync(r =>
                    r.WishListId == wishListId &&
                    r.RequestedByUserId == userId &&
                    r.Status == AccessRequestStatus.Approved),

            WishListVisibility.Private => false,
            _ => false
        };
    }
}