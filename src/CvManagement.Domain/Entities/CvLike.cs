namespace CvManagement.Domain.Entities;

public class CvLike : VersionedEntity
{
    public Guid CvId { get; set; }
    public Cv Cv { get; set; } = null!;
    public string RecruiterUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
