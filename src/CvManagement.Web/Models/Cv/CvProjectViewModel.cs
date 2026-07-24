namespace CvManagement.Web.Models.Cv;

public class CvProjectViewModel
{
    public string Name { get; set; } = string.Empty;
    public string PeriodDisplay { get; set; } = string.Empty;
    public string DescriptionMarkdown { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
