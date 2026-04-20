using Microsoft.EntityFrameworkCore;
using WishListApp.Models;

namespace WishListApp;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<WishList> WishLists => Set<WishList>();
    public DbSet<WishListItem> WishListItems => Set<WishListItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One-to-one: WishListItem → Booking
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Item)
            .WithOne(i => i.Booking)
            .HasForeignKey<Booking>(b => b.ItemId);

        // Friendship: two FKs both pointing to User — EF can't guess this
        // so you must configure it explicitly
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);  // Restrict prevents cascade delete cycles

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Addressee)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(f => f.AddresseeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate friend pairs
        modelBuilder.Entity<Friendship>()
            .HasIndex(f => new { f.RequesterId, f.AddresseeId })
            .IsUnique();

        // Decimal precision
        modelBuilder.Entity<WishListItem>()
            .Property(i => i.EstimatedPrice)
            .HasPrecision(10, 2);
    }
}