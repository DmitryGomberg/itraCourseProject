namespace CvManagement.Domain.Entities;

public class DiscussionPost : VersionedEntity
{
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    public string AuthorUserId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
