namespace WishListApp.Models;

public class WishListItem
{
    public Guid Id { get; set; }
    public Guid WishListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ShoppingLink { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public string? Notes { get; set; }

    public WishList WishList { get; set; } = null!;
    public Booking? Booking { get; set; }
}