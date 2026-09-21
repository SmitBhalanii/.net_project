using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => w.CustomerId).IsUnique();

        builder.HasMany(w => w.WishlistItems)
            .WithOne(wi => wi.Wishlist)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
