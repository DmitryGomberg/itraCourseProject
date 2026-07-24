namespace CvManagement.Domain.Entities;

public class PositionAccessRule : VersionedEntity
{
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    public Guid AttributeDefinitionId { get; set; }
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public AccessRuleOperator Operator { get; set; }
    public string ComparisonValue { get; set; } = string.Empty;
}
