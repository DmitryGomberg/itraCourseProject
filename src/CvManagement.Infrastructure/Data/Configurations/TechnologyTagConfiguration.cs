using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class TechnologyTagConfiguration : IEntityTypeConfiguration<TechnologyTag>
{
    public void Configure(EntityTypeBuilder<TechnologyTag> builder)
    {
        builder.ConfigureVersioning();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Positions)
            .WithMany(p => p.RelevantProjectTags)
            .UsingEntity(j => j.ToTable("PositionTechnologyTags"));
    }
}
