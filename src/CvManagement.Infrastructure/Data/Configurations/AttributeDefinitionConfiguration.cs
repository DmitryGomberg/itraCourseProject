using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ConfigureVersioning();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Options)
            .WithOne(o => o.AttributeDefinition)
            .HasForeignKey(o => o.AttributeDefinitionId);
    }
}
