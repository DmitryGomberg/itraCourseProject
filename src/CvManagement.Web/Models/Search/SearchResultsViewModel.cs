namespace CvManagement.Web.Models.Search;

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public List<PositionSearchItem> PositionResults { get; set; } = new();
    public List<CvSearchItem> CvResults { get; set; } = new();
}

public class PositionSearchItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
}

public class CvSearchItem
{
    public Guid Id { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public int LikesCount { get; set; }
}
