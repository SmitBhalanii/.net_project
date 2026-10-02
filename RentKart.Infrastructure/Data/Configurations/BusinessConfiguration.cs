using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.HasIndex(b => b.City);
        builder.HasIndex(b => b.ApprovalStatus);
        
        builder.Property(b => b.BusinessName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.OwnerName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Email).HasMaxLength(150).IsRequired();
        builder.Property(b => b.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Address).HasMaxLength(250).IsRequired();
        builder.Property(b => b.City).HasMaxLength(100).IsRequired();
        builder.Property(b => b.State).HasMaxLength(100).IsRequired();
        builder.Property(b => b.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(2000);
        builder.Property(b => b.Website).HasMaxLength(250);
        builder.Property(b => b.LogoPath).HasMaxLength(500);
        builder.Property(b => b.CoverImagePath).HasMaxLength(500);
        builder.Property(b => b.BusinessHours).HasMaxLength(500);

        builder.HasMany(b => b.Equipment)
            .WithOne(e => e.Business)
            .HasForeignKey(e => e.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Bookings)
            .WithOne(bk => bk.Business)
            .HasForeignKey(bk => bk.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Reviews)
            .WithOne(r => r.Business)
            .HasForeignKey(r => r.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
