using WishListApp.DTOs;
using WishListApp.Models;

namespace WishListApp.Interfaces;

public interface IFriendshipService
{
    Task<bool> SendRequestAsync(Guid requesterId, Guid addresseeId);
    Task<bool> RespondToRequestAsync(Guid friendshipId, Guid userId, FriendshipStatus response);
    Task<List<UserDtos.UserResponse>> GetFriendsAsync(Guid userId);
    Task<List<FriendshipDtos.FriendshipResponse>> GetPendingRequestsAsync(Guid userId);
}