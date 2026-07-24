namespace CvManagement.Web.Models.CandidatePositions;

public class AvailablePositionViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public Guid? ExistingCvId { get; set; }
    public string? ExistingCvStatus { get; set; }
}
