using CvManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagement.Infrastructure.Data.Configurations;

public class PositionAccessRuleConfiguration : IEntityTypeConfiguration<PositionAccessRule>
{
    public void Configure(EntityTypeBuilder<PositionAccessRule> builder)
    {
        builder.ConfigureVersioning();
    }
}
