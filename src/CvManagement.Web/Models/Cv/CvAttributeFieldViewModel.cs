namespace CvManagement.Web.Models.Cv;

public class CvAttributeFieldViewModel
{
    public Guid AttributeDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? StringValue { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public List<(Guid Id, string Value)> Options { get; set; } = new();
    public bool IsEmpty { get; set; }
}
