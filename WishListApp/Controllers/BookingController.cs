using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WishListApp.Interfaces;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/items/{itemId:guid}/booking")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST /api/items/{itemId}/booking
    [HttpPost]
    public async Task<IActionResult> BookItem(Guid itemId)
    {
        var success = await _bookingService.BookItemAsync(itemId, GetCurrentUserId());

        return success ? NoContent() : BadRequest("Item not found, already booked, or you own this wishlist.");
    }

    // DELETE /api/items/{itemId}/booking
    [HttpDelete]
    public async Task<IActionResult> UnbookItem(Guid itemId)
    {
        var success = await _bookingService.UnbookItemAsync(itemId, GetCurrentUserId());

        return success ? NoContent() : NotFound("Booking not found or you did not make this booking.");
    }

    private Guid GetCurrentUserId() => Guid.Parse("00000000-0000-0000-0000-000000000001");
}