namespace CvManagement.Web.Models.Positions;

public class PositionIndexViewModel
{
    public List<PositionListItemViewModel> Items { get; set; } = new();
    public string? Search { get; set; }
}
