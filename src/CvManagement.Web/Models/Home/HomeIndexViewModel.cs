using CvManagement.Web.Models.Positions;

namespace CvManagement.Web.Models.Home;

public class HomeIndexViewModel
{
    public int TotalCandidates { get; set; }
    public int TotalPositions { get; set; }
    public int TotalPublishedCvs { get; set; }
    public List<PositionListItemViewModel> LatestPositions { get; set; } = new();
    public List<TopPositionItem> TopPositions { get; set; } = new();
    public List<TagCloudItem> TagCloud { get; set; } = new();
}

public class TopPositionItem
{
    public string Title { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public int CvCount { get; set; }
}

public class TagCloudItem
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
