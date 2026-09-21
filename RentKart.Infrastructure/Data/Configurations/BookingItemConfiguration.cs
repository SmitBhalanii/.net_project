using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.HasKey(bi => bi.Id);
        
        builder.Property(bi => bi.RentalPricePerDay).HasColumnType("decimal(18,2)");
        builder.Property(bi => bi.Subtotal).HasColumnType("decimal(18,2)");

        builder.HasOne(bi => bi.Equipment)
            .WithMany(e => e.BookingItems)
            .HasForeignKey(bi => bi.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
