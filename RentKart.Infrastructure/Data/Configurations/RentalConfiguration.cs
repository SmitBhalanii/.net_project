using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.BookingId).IsUnique();
        builder.HasIndex(r => r.EquipmentId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.BusinessId);
        builder.HasIndex(r => r.Status);

        builder.Property(r => r.IssueNotes).HasMaxLength(500);
        builder.Property(r => r.ReturnNotes).HasMaxLength(500);
        builder.Property(r => r.DamageDescription).HasMaxLength(1000);

        builder.HasOne(r => r.Booking)
            .WithOne(b => b.Rental)
            .HasForeignKey<Rental>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Equipment)
            .WithMany() // Can add to Equipment if needed
            .HasForeignKey(r => r.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Business)
            .WithMany()
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.IssuedByStaff)
            .WithMany()
            .HasForeignKey(r => r.IssuedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReturnedToStaff)
            .WithMany()
            .HasForeignKey(r => r.ReturnedToStaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
