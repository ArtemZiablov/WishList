namespace WishListApp.DTOs;

public class WishListItemDtos
{
    public record WishListItemResponse(
        Guid Id,
        Guid WishListId,
        string Title,
        string? Description,
        string? ImageUrl,
        string? ShoppingLink,
        decimal? EstimatedPrice,
        string? Notes,
        bool IsBooked
    );

    public record CreateWishListItemRequest(
        string Title,
        string? Description,
        string? ImageUrl,
        string? ShoppingLink,
        decimal? EstimatedPrice,
        string? Notes
    );

    public record UpdateWishListItemRequest(
        string Title,
        string? Description,
        string? ImageUrl,
        string? ShoppingLink,
        decimal? EstimatedPrice,
        string? Notes
    );
}