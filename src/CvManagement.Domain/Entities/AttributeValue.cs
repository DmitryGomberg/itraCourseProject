namespace CvManagement.Domain.Entities;

public class AttributeValue : VersionedEntity
{
    public Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public Guid AttributeDefinitionId { get; set; }
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public string? StringValue { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public AttributeOption? SelectedOption { get; set; }
}
