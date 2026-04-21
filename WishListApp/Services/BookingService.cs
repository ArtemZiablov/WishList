using Microsoft.EntityFrameworkCore;
using WishListApp.Interfaces;
using WishListApp.Models;

namespace WishListApp.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    
    public BookingService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<bool> BookItemAsync(Guid itemId, Guid bookedByUserId)
    {
        var item = await _db.WishListItems
            .Include(i => i.Booking)
            .Include(i => i.WishList)
            .FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return false;
        if (item.Booking is not null) return false;
        if (item.WishList.OwnerId == bookedByUserId) return false;
        
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            BookedByUserId = bookedByUserId,
            BookedAt = DateTime.UtcNow,
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnbookItemAsync(Guid itemId, Guid userId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.ItemId == itemId && b.BookedByUserId == userId);
        
        if (booking is null) return false;
        
        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        return true;
    }
}