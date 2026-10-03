using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasOne(w => w.Customer)
            .WithMany()
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Equipment)
            .WithMany(e => e.WishlistItems)
            .HasForeignKey(w => w.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.CustomerId, w.EquipmentId }).IsUnique();
    }
}
