using System.ComponentModel.DataAnnotations;

namespace CvManagement.Web.Models.Projects;

public class ProjectFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Введите название проекта")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly PeriodStart { get; set; }

    public bool IsOngoing { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    [Required(ErrorMessage = "Введите описание проекта")]
    public string DescriptionMarkdown { get; set; } = string.Empty;

    public string? TagsInput { get; set; }

    public uint? Version { get; set; }
}
