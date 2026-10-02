using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.HasIndex(b => b.BookingNumber).IsUnique();
        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.BusinessId);
        builder.HasIndex(b => b.EquipmentId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.StartDate);
        builder.HasIndex(b => b.EndDate);

        builder.Property(b => b.BookingNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.DailyRate).HasColumnType("decimal(18,2)");
        builder.Property(b => b.RentalAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SecurityDepositAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.PaidAmount).HasColumnType("decimal(18,2)");

        builder.Property(b => b.CustomerNote).HasMaxLength(500);
        builder.Property(b => b.BusinessNote).HasMaxLength(500);

        builder.HasOne(b => b.Equipment)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Business)
            .WithMany() // Assuming Business doesn't have Bookings collection explicitly, or maybe it does
            .HasForeignKey(b => b.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Customer)
            .WithMany() // ApplicationUser
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

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
