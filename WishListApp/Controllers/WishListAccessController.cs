using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Controllers;


[ApiController]
[Route("api/wishlists")]
public class WishListAccessController : BaseController
{
    private readonly IWishListAccessService _accessService;

    public WishListAccessController(IWishListAccessService accessService)
    {
        _accessService = accessService;
    }

    // GET /api/wishlists/public/{token}
    // No auth needed — anyone with the public link can view
    [AllowAnonymous]
    [HttpGet("public/{token}")]
    public async Task<ActionResult<WishListDtos.WishListResponse>> GetByPublicToken(string token)
    {
        var result = await _accessService.GetByPublicTokenAsync(token);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/wishlists/invite/{inviteToken}
    // Must be logged in — shows info about the list before requesting access
    [HttpGet("invite/{inviteToken}")]
    public async Task<ActionResult<WishListAccessDtos.InviteLinkInfoResponse>> GetInviteInfo(string inviteToken)
    {
        var result = await _accessService.GetInviteLinkInfoAsync(inviteToken);
        return result is null ? NotFound("Invite link is invalid or the list is not set to invite-only.") : Ok(result);
    }

    // POST /api/wishlists/invite/{inviteToken}/request
    // Logged-in user requests access after seeing invite info
    [HttpPost("invite/{inviteToken}/request")]
    public async Task<IActionResult> RequestAccess(string inviteToken)
    {
        var success = await _accessService.RequestAccessAsync(inviteToken, GetCurrentUserId());
        return success ? NoContent() : BadRequest("Could not request access. Link may be invalid or you already have access.");
    }

    // GET /api/wishlists/access-requests
    // Owner sees all pending requests across all their lists
    [HttpGet("access-requests")]
    public async Task<ActionResult<List<WishListAccessDtos.AccessRequestResponse>>> GetPendingRequests()
    {
        var requests = await _accessService.GetPendingRequestsAsync(GetCurrentUserId());
        return Ok(requests);
    }

    // PUT /api/wishlists/access-requests/{requestId}
    // Owner approves or rejects a specific request
    [HttpPut("access-requests/{requestId:guid}")]
    public async Task<IActionResult> RespondToRequest(
        Guid requestId,
        [FromBody] AccessRequestStatus response)
    {
        var success = await _accessService.RespondToRequestAsync(requestId, GetCurrentUserId(), response);
        return success ? NoContent() : BadRequest("Could not respond to this request.");
    }
}
