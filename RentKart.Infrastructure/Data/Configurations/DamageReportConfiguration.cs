using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class DamageReportConfiguration : IEntityTypeConfiguration<DamageReport>
{
    public void Configure(EntityTypeBuilder<DamageReport> builder)
    {
        builder.HasKey(dr => dr.Id);
        builder.Property(dr => dr.EstimatedCost).HasColumnType("decimal(18,2)");
    }
}
