using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class WishListService : IWishListService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    
    public WishListService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<WishListDtos.WishListResponse>> GetAllByUserAsync(Guid userId)
    {
        var wishLists = await _db.WishLists
            .Where(w => w.OwnerId == userId)
            .Include(w => w.Items)  // needed so ItemCount works
            .ToListAsync();

        return _mapper.Map<List<WishListDtos.WishListResponse>>(wishLists);
    }

    public async Task<WishListDtos.WishListResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var wishList = await _db.WishLists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == userId);
        
        return wishList is null ? null : _mapper.Map<WishListDtos.WishListResponse>(wishList);
    }

    public async Task<WishListDtos.WishListResponse> CreateAsync(Guid userId, WishListDtos.CreateWishListRequest request)
    {
        var wishList = _mapper.Map<WishList>(request);
        wishList.OwnerId = userId;
        
        _db.WishLists.Add(wishList);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<WishListDtos.WishListResponse>(wishList);
    }

    public async Task<WishListDtos.WishListResponse?> UpdateAsync(Guid id, Guid userId, WishListDtos.UpdateWishListRequest request)
    {
        var wishList = _db.WishLists
            .Include(w => w.Items)
            .FirstOrDefault(w => w.Id == id && w.OwnerId == userId);
        
        if (wishList is null) return null;
        
        _mapper.Map(request, wishList);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<WishListDtos.WishListResponse>(wishList);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var wishList = await _db.WishLists
            .FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == userId);
        
        if (wishList is null) return false;
        
        _db.WishLists.Remove(wishList);
        await _db.SaveChangesAsync();
        
        return true;
    }
}
