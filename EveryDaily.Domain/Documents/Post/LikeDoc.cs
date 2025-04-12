using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class LikeDoc : IEntityBase<ObjectId>
{
    public ObjectId Id { get; set; }
    public Guid UserId { get; set; }
    public ObjectId PostId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}