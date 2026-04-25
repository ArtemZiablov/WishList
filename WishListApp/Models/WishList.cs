namespace WishListApp.Models;

public class WishList
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public string ShareToken { get; set; } = Guid.NewGuid().ToString("N");
    public string InviteToken { get; set; } = Guid.NewGuid().ToString("N"); 
    public WishListVisibility Visibility { get; set; } = WishListVisibility.Private;
    
    public EventType EventType { get; set; }
    public DateTime? EventDate { get; set; }
    
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    public List<WishListItem> Items { get; set; } = new List<WishListItem>();
    
}

