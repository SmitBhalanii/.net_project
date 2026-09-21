using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.BusinessId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.StartDate);
        builder.HasIndex(b => b.EndDate);

        builder.Property(b => b.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SecurityDeposit).HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasMany(b => b.BookingItems)
            .WithOne(bi => bi.Booking)
            .HasForeignKey(bi => bi.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.RentalAgreement)
            .WithOne(ra => ra.Booking)
            .HasForeignKey<RentalAgreement>(ra => ra.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Invoice)
            .WithOne(i => i.Booking)
            .HasForeignKey<Invoice>(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Reviews)
            .WithOne(r => r.Booking)
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.DamageReports)
            .WithOne(dr => dr.Booking)
            .HasForeignKey(dr => dr.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
