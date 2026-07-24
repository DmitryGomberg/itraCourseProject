namespace CvManagement.Web.Models.Positions;

public class ManageTagsViewModel
{
    public Guid PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public List<TagItem> CurrentTags { get; set; } = new();
}

public class TagItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
