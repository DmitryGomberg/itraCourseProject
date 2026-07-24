namespace CvManagement.Web.Models.Positions;

public class PositionListItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescriptionShort { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxProjects { get; set; }
    public int AttributesCount { get; set; }
    public int AccessRulesCount { get; set; }
}
