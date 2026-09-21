namespace RentKart.Core.Entities;

public class WishlistItem
{
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public Wishlist Wishlist { get; set; } = null!;
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
}
