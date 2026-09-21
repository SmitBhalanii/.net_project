using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(wi => wi.Id);
        
        // Prevent duplicate equipment in same wishlist
        builder.HasIndex(wi => new { wi.WishlistId, wi.EquipmentId }).IsUnique();

        builder.HasOne(wi => wi.Equipment)
            .WithMany(e => e.WishlistItems)
            .HasForeignKey(wi => wi.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
