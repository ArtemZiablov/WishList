using Microsoft.AspNetCore.Mvc;
using WishListApp.DTOs;
using WishListApp.Interfaces;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/wishlists/{wishListId:guid}/items")]  // nested route
public class WishListItemsController : ControllerBase
{
    private readonly IWishListItemService _itemService;

    public WishListItemsController(IWishListItemService itemService)
    {
        _itemService = itemService;
    }

    // GET /api/wishlists/{wishListId}/items
    [HttpGet]
    public async Task<ActionResult<List<WishListItemDtos.WishListItemResponse>>> GetAll(Guid wishListId)
    {
        var items = await _itemService.GetAllByWishListAsync(wishListId, GetCurrentUserId());
        return Ok(items);
    }

    // GET /api/wishlists/{wishListId}/items/{itemId}
    [HttpGet("{itemId:guid}")]
    public async Task<ActionResult<WishListItemDtos.WishListItemResponse>> GetById(Guid wishListId, Guid itemId)
    {
        var item = await _itemService.GetByIdAsync(itemId, GetCurrentUserId());
        return item is null ? NotFound() : Ok(item);
    }

    // POST /api/wishlists/{wishListId}/items
    [HttpPost]
    public async Task<ActionResult<WishListItemDtos.WishListItemResponse>> Create(
        Guid wishListId,
        WishListItemDtos.CreateWishListItemRequest request)
    {
        var result = await _itemService.CreateAsync(wishListId, GetCurrentUserId(), request);

        if (result is null) return NotFound("Wishlist not found or access denied");

        return CreatedAtAction(nameof(GetById), new { wishListId, itemId = result.Id }, result);
    }

    // PUT /api/wishlists/{wishListId}/items/{itemId}
    [HttpPut("{itemId:guid}")]
    public async Task<ActionResult<WishListItemDtos.WishListItemResponse>> Update(
        Guid wishListId,
        Guid itemId,
        WishListItemDtos.UpdateWishListItemRequest request)
    {
        var result = await _itemService.UpdateAsync(itemId, GetCurrentUserId(), request);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE /api/wishlists/{wishListId}/items/{itemId}
    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Delete(Guid wishListId, Guid itemId)
    {
        var deleted = await _itemService.DeleteAsync(itemId, GetCurrentUserId());
        return deleted ? NoContent() : NotFound();
    }

    private Guid GetCurrentUserId() => Guid.Parse("00000000-0000-0000-0000-000000000001");
}