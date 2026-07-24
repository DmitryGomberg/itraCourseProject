namespace CvManagement.Domain.Entities;

public class Position : VersionedEntity
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxProjects { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<PositionAttribute> PositionAttributes { get; set; } = new List<PositionAttribute>();
    public ICollection<PositionAccessRule> AccessRules { get; set; } = new List<PositionAccessRule>();
    public ICollection<TechnologyTag> RelevantProjectTags { get; set; } = new List<TechnologyTag>();
    public ICollection<Cv> Cvs { get; set; } = new List<Cv>();
}
