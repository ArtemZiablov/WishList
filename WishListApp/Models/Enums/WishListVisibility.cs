namespace WishListApp.Models;

public enum WishListVisibility
{
    Private,   // only owner
    Public,    // anyone with public link, no approval needed
    Invite     // private link, requires access request + owner approval
}