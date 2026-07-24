using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Positions;

public class AddTagsRequest
{
    [Required]
    public string TagName { get; set; } = string.Empty;
}
