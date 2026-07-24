namespace CvManagement.Domain.Entities;

public class Project : VersionedEntity
{
    public Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public string DescriptionMarkdown { get; set; } = string.Empty;
    public ICollection<TechnologyTag> Tags { get; set; } = new List<TechnologyTag>();
}
