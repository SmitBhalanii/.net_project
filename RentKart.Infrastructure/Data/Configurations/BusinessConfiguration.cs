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
