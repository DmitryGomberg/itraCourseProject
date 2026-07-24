using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Positions;

public class PositionFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Short description")]
    public string ShortDescription { get; set; } = string.Empty;

    [Display(Name = "Public")]
    public bool IsPublic { get; set; }

    [Range(0, 50)]
    [Display(Name = "Max projects")]
    public int MaxProjects { get; set; }

    public uint? Version { get; set; }
}
