namespace WishListApp.Interfaces;

public interface IBookingService
{
    Task<bool> BookItemAsync(Guid itemId, Guid bookedByUserId);

    Task<bool> UnbookItemAsync(Guid itemId, Guid userId);
}