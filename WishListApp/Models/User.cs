namespace WishListApp.Models;
using Microsoft.AspNetCore.Identity;

public class User: IdentityUser<Guid>
{
    /*public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;*/
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<WishList> WishLists { get; set; } = new List<WishList>();
    public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
}