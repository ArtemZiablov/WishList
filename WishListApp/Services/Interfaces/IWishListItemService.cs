using WishListApp.DTOs;

namespace WishListApp.Interfaces;

public interface IWishListItemService
{
    Task<List<WishListItemDtos.WishListItemResponse>> GetAllByWishListAsync(Guid wishListId, Guid userId);
    Task<WishListItemDtos.WishListItemResponse?> GetByIdAsync(Guid itemId, Guid userId);
    Task<WishListItemDtos.WishListItemResponse?> CreateAsync(Guid wishListId, Guid userId, WishListItemDtos.CreateWishListItemRequest request);
    Task<WishListItemDtos.WishListItemResponse?> UpdateAsync(Guid itemId, Guid userId, WishListItemDtos.UpdateWishListItemRequest request);
    Task<bool> DeleteAsync(Guid itemId, Guid userId);
}