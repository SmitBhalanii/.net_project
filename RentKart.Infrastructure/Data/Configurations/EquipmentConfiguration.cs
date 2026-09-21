using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.BusinessId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsActive);

        builder.Property(e => e.RentalPricePerDay).HasColumnType("decimal(18,2)");
        builder.Property(e => e.SecurityDeposit).HasColumnType("decimal(18,2)");

        builder.HasMany(e => e.EquipmentImages)
            .WithOne(ei => ei.Equipment)
            .HasForeignKey(ei => ei.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MaintenanceRecords)
            .WithOne(mr => mr.Equipment)
            .HasForeignKey(mr => mr.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DamageReports)
            .WithOne(dr => dr.Equipment)
            .HasForeignKey(dr => dr.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Reviews)
            .WithOne(r => r.Equipment)
            .HasForeignKey(r => r.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
