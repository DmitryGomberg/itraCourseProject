namespace CvManagement.Web.Models.Positions;

public class ManageAttributesViewModel
{
    public Guid PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public List<AttributeCheckItem> AllAttributes { get; set; } = new();
}

public class AttributeCheckItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DataTypeDisplay { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
