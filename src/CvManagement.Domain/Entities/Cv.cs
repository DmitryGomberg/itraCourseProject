namespace CvManagement.Domain.Entities;

public class Cv : VersionedEntity
{
    public Guid ProfileId { get; set; }
    public Profile Profile { get; set; } = null!;
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = null!;
    public CvStatus Status { get; set; } = CvStatus.Draft;
}
