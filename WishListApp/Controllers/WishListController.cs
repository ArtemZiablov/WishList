using Microsoft.AspNetCore.Mvc;
using WishListApp.DTOs;
using WishListApp.Interfaces;

namespace WishListApp.Controllers;

[ApiController]
[Route("api/[controller]")]  // /api/wishlist
public class WishListController : BaseController
{
    private readonly IWishListService _wishListService;

    public WishListController(IWishListService wishListService)
    {
        _wishListService = wishListService;
    }

    // GET /api/wishlist
    [HttpGet]
    public async Task<ActionResult<List<WishListDtos.WishListResponse>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var wishlists = await _wishListService.GetAllByUserAsync(userId);
        return Ok(wishlists);
    }

    // GET /api/wishlist/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WishListDtos.WishListResponse>> GetById(Guid id)
    {
        var result = await _wishListService.GetByIdAsync(id, GetCurrentUserId());

        return result is null ? NotFound() : Ok(result);
    }

    // POST /api/wishlist
    [HttpPost]
    public async Task<ActionResult<WishListDtos.WishListResponse>> Create(WishListDtos.CreateWishListRequest request)
    {
        var result = await _wishListService.CreateAsync(GetCurrentUserId(), request);

        // 201 Created — includes a Location header pointing to the new resource
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT /api/wishlist/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WishListDtos.WishListResponse>> Update(Guid id, WishListDtos.UpdateWishListRequest request)
    {
        var result = await _wishListService.UpdateAsync(id, GetCurrentUserId(), request);

        return result is null ? NotFound() : Ok(result);
    }

    // DELETE /api/wishlist/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _wishListService.DeleteAsync(id, GetCurrentUserId());

        return deleted ? NoContent() : NotFound();
    }
}
