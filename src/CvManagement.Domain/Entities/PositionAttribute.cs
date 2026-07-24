namespace CvManagement.Domain.Entities;

public class PositionAttribute : VersionedEntity
{
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    public Guid AttributeDefinitionId { get; set; }
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public int DisplayOrder { get; set; }
}
