namespace CvManagement.Domain.Entities;

public class AttributeOption : VersionedEntity
{
    public Guid AttributeDefinitionId { get; set; }
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
}
