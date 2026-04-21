using System.ComponentModel.DataAnnotations.Schema;

namespace WishListApp.Models;

public class Booking
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid BookedByUserId { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public WishListItem Item { get; set; } = null!;
    [ForeignKey(nameof(BookedByUserId))]
    public User BookedBy { get; set; } = null!;
    
}