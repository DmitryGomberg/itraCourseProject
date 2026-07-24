using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Profile;

public class UpdateAttributeValueRequest
{
    [Required]
    public Guid AttributeDefinitionId { get; set; }

    public string? StringValue { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
}
