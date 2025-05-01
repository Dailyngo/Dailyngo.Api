using MongoDB.Bson;

namespace EveryDaily.Core.Entity;

public abstract class EntityBase : IEntityBase<Guid>
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
public abstract class DocBase : IEntityBase<ObjectId>
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ObjectId Id { get; set; }
}