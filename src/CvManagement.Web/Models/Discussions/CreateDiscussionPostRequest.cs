using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Discussions;

public class CreateDiscussionPostRequest
{
    public Guid PositionId { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;
}
