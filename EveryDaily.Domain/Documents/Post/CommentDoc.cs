using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class CommentDoc : IEntityBase<ObjectId>
{
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public ObjectId PostId { get; set; }
    public ObjectId? ReplyCommentId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ObjectId Id { get; set; }
}