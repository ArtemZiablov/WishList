using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class FriendshipService: IFriendshipService
{
    public async Task<bool> SendRequestAsync(Guid requesterId, Guid addresseeId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RespondToRequestAsync(Guid friendshipId, Guid userId, FriendshipStatus response)
    {
        throw new NotImplementedException();
    }

    public async Task<List<UserDtos.UserResponse>> GetFriendsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<FriendshipDtos.FriendshipResponse>> GetPendingRequestsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}