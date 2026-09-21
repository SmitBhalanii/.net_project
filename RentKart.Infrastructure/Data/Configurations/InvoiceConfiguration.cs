using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.SecurityDeposit).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
    }
}
