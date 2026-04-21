using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class FriendshipService : IFriendshipService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public FriendshipService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<bool> SendRequestAsync(Guid requesterId, Guid addresseeId)
    {
        if (requesterId == addresseeId) return false;

        var addresseeExists = await _db.Users.AnyAsync(u => u.Id == addresseeId);
        if (!addresseeExists) return false;

        // Check no friendship already exists in either direction
        var alreadyExists = await _db.Friendships.AnyAsync(f =>
            (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
            (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

        if (alreadyExists) return false;

        var friendship = new Friendship
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending,
        };

        _db.Friendships.Add(friendship);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RespondToRequestAsync(Guid friendshipId, Guid userId, FriendshipStatus response)
    {
        // Only the person who received the request can respond to it
        var friendship = await _db.Friendships
            .FirstOrDefaultAsync(f => f.Id == friendshipId && f.AddresseeId == userId);

        if (friendship is null) return false;

        if (friendship.Status != FriendshipStatus.Pending) return false;

        // Only Accepted or Rejected are valid responses — can't respond with Pending
        if (response == FriendshipStatus.Pending) return false;

        friendship.Status = response;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserDtos.UserResponse>> GetFriendsAsync(Guid userId)
    {
        var friendships = await _db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f =>
                (f.RequesterId == userId || f.AddresseeId == userId) &&
                f.Status == FriendshipStatus.Accepted)
            .ToListAsync();

        // From each friendship, extract the "other" person — not the current user
        var friends = friendships
            .Select(f => f.RequesterId == userId ? f.Addressee : f.Requester)
            .ToList();

        return _mapper.Map<List<UserDtos.UserResponse>>(friends);
    }

    public async Task<List<FriendshipDtos.FriendshipResponse>> GetPendingRequestsAsync(Guid userId)
    {
        // Only return requests addressed TO this user — not ones they sent
        var pending = await _db.Friendships
            .Include(f => f.Requester)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();

        return _mapper.Map<List<FriendshipDtos.FriendshipResponse>>(pending);
    }
}