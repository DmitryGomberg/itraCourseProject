namespace CvManagement.Web.Models.Cv;

public class CvSummaryViewModel
{
    public Guid Id { get; set; }
    public Guid PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public string PositionShortDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public bool CanEdit { get; set; }
    public bool CanPublish { get; set; }
    public bool CanUnpublish { get; set; }
    public List<CvAttributeFieldViewModel> Attributes { get; set; } = new();
    public List<CvProjectViewModel> Projects { get; set; } = new();
    public uint Version { get; set; }
    public int LikesCount { get; set; }
    public bool LikedByCurrentUser { get; set; }
    public bool CanLike { get; set; }
}
