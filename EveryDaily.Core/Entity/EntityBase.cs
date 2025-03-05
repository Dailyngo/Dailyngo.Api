namespace EveryDaily.Core.Entity;

public abstract class EntityBase : IEntityBase<Guid>
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}