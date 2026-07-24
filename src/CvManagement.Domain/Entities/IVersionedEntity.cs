namespace CvManagement.Domain.Entities;

public interface IVersionedEntity
{
    Guid Id { get; set; }
    uint Version { get; set; }
}
