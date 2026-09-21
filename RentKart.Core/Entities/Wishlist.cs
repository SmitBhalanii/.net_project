using System.Collections.Generic;

namespace RentKart.Core.Entities;

public class Wishlist
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
    
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
