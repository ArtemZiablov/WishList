using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WishListApp.DTOs;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class WishListItemService: IWishListItemService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public WishListItemService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    
    public async Task<List<WishListItemDtos.WishListItemResponse>> GetAllByWishListAsync(Guid wishListId, Guid userId)
    {
        var wishListExists = await _db.WishLists
            .AnyAsync(w => w.Id == wishListId && w.OwnerId == userId);
        
        if (!wishListExists) return new List<WishListItemDtos.WishListItemResponse>();

        var items = await _db.WishListItems
            .Include(i => i.Booking)
            .Where(i => i.WishListId == wishListId)
            .ToListAsync();

        return _mapper.Map<List<WishListItemDtos.WishListItemResponse>>(items);
    }

    public async Task<WishListItemDtos.WishListItemResponse?> GetByIdAsync(Guid itemId, Guid userId)
    {
        var item = await _db.WishListItems
            .Include(i => i.Booking)
            .Include(i => i.WishList)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.WishList.OwnerId == userId);
        
        return _mapper.Map<WishListItemDtos.WishListItemResponse>(item);
    }

    public async Task<WishListItemDtos.WishListItemResponse?> CreateAsync(Guid wishListId, Guid userId,
        WishListItemDtos.CreateWishListItemRequest request)
    {
        var list = await _db.WishLists.AnyAsync(w => w.Id == wishListId && w.OwnerId == userId);
        if (!list) return null;

        var item = _mapper.Map<WishListItem>(request);
        item.WishListId = wishListId;

        _db.WishListItems.Add(item);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<WishListItemDtos.WishListItemResponse>(item);
    }


    public async Task<WishListItemDtos.WishListItemResponse?> UpdateAsync(Guid itemId, Guid userId, WishListItemDtos.UpdateWishListItemRequest request)
    {
        var item = await _db.WishListItems
            .Include(i => i.WishList)
            .Include(i => i.Booking)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.WishList.OwnerId == userId);

        if (item is null) return null;
        
        _mapper.Map(request, item);
        await _db.SaveChangesAsync();

        return _mapper.Map<WishListItemDtos.WishListItemResponse>(item);
    }

    public async Task<bool> DeleteAsync(Guid itemId, Guid userId)
    {
        var item = await _db.WishListItems
            .Include(i => i.WishList)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.WishList.OwnerId == userId);

        if (item is null) return false;

        _db.WishListItems.Remove(item);
        await _db.SaveChangesAsync();

        return true;
    }
}