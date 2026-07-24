namespace CvManagement.Domain.Entities;

public class AttributeDefinition : VersionedEntity
{
    public AttributeCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AttributeDataType DataType { get; set; }
    public ICollection<AttributeOption> Options { get; set; } = new List<AttributeOption>();
    public DateTimeOffset? LastUsedAt { get; set; }
}
