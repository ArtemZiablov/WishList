namespace WishListApp.Models;

public class WishListAccessRequest
{
    public Guid Id { get; set; }
    public Guid WishListId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public WishList WishList { get; set; } = null!;
    public User RequestedBy { get; set; } = null!;
}