using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class CvLikeConfiguration : IEntityTypeConfiguration<CvLike>
{
    public void Configure(EntityTypeBuilder<CvLike> builder)
    {
        builder.ConfigureVersioning();

        builder.HasIndex(x => new { x.CvId, x.RecruiterUserId }).IsUnique();
    }
}
