using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentKart.Core.Entities;

namespace RentKart.Infrastructure.Data.Configurations;

public class RentalAgreementConfiguration : IEntityTypeConfiguration<RentalAgreement>
{
    public void Configure(EntityTypeBuilder<RentalAgreement> builder)
    {
        builder.HasKey(ra => ra.Id);
        builder.HasIndex(ra => ra.AgreementNumber).IsUnique();
    }
}
