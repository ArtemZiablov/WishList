using WishListApp.DTOs;

namespace WishListApp.Interfaces;

public interface IWishListService
{
    Task<List<WishListDtos.WishListResponse>> GetAllByUserAsync(Guid userId);
    Task<WishListDtos.WishListResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<WishListDtos.WishListResponse> CreateAsync(Guid userId, WishListDtos.CreateWishListRequest request);
    Task<WishListDtos.WishListResponse?> UpdateAsync(Guid id, Guid userId, WishListDtos.UpdateWishListRequest request);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}