using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Profile;

public class UpdateProfileFieldRequest
{
    [Required]
    public string Field { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    [Required]
    public uint Version { get; set; }
}
