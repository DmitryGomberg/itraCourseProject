using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ConfigureVersioning();

        builder.HasMany(x => x.Tags)
            .WithMany(t => t.Projects)
            .UsingEntity(j => j.ToTable("ProjectTechnologyTags"));
    }
}
