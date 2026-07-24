namespace CvManagement.Web.Models.Projects;

public class ProjectListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PeriodDisplay { get; set; } = string.Empty;
    public string TagsDisplay { get; set; } = string.Empty;
    public string DescriptionShort { get; set; } = string.Empty;
}
