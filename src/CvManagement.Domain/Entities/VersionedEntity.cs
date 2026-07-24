namespace CvManagement.Domain.Entities;

public abstract class VersionedEntity : IVersionedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public uint Version { get; set; }
}
