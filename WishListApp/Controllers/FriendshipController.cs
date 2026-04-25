using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/friends")]
public class FriendshipController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    // GET /api/friends
    [HttpGet]
    public async Task<ActionResult<List<UserDtos.UserResponse>>> GetFriends()
    {
        var friends = await _friendshipService.GetFriendsAsync(GetCurrentUserId());
        return Ok(friends);
    }

    // GET /api/friends/requests
    [HttpGet("requests")]
    public async Task<ActionResult<List<FriendshipDtos.FriendshipResponse>>> GetPendingRequests()
    {
        var requests = await _friendshipService.GetPendingRequestsAsync(GetCurrentUserId());
        return Ok(requests);
    }

    // POST /api/friends/request
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] Guid addresseeId)
    {
        var success = await _friendshipService.SendRequestAsync(GetCurrentUserId(), addresseeId);

        return success ? NoContent() : BadRequest("Could not send friend request. User may not exist or request already exists.");
    }

    // PUT /api/friends/requests/{friendshipId}
    [HttpPut("requests/{friendshipId:guid}")]
    public async Task<IActionResult> RespondToRequest(
        Guid friendshipId,
        [FromBody] FriendshipStatus response)
    {
        var success = await _friendshipService.RespondToRequestAsync(friendshipId, GetCurrentUserId(), response);

        return success ? NoContent() : BadRequest("Could not respond to this request.");
    }

    private Guid GetCurrentUserId() => Guid.Parse("00000000-0000-0000-0000-000000000001");
}