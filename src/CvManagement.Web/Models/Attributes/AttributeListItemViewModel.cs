namespace CvManagement.Web.Models.Attributes;

public class AttributeListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DataTypeDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OptionsCount { get; set; }
}
