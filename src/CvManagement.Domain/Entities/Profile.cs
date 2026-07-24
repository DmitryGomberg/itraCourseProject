namespace CvManagement.Domain.Entities;

public class Profile : VersionedEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
