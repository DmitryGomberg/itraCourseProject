namespace CvManagement.Web.Models.Discussions;

public class DiscussionPostViewModel
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid? AuthorProfileId { get; set; }
    public string? AuthorPhotoUrl { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
