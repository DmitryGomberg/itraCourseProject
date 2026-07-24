using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ConfigureVersioning();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasMany(x => x.Projects)
            .WithOne(p => p.Profile)
            .HasForeignKey(p => p.ProfileId);

        builder.HasMany(x => x.AttributeValues)
            .WithOne(v => v.Profile)
            .HasForeignKey(v => v.ProfileId);
    }
}
