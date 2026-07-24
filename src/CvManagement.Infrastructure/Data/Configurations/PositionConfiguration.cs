using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ConfigureVersioning();

        builder.HasMany(x => x.PositionAttributes)
            .WithOne(pa => pa.Position)
            .HasForeignKey(pa => pa.PositionId);

        builder.HasMany(x => x.AccessRules)
            .WithOne(r => r.Position)
            .HasForeignKey(r => r.PositionId);
    }
}
