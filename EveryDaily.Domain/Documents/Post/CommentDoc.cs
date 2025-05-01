using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class CommentDoc : DocBase
{
    public string UserId { get; set; }
    public string Content { get; set; }
    public ObjectId PostId { get; set; }
    public ObjectId? ReplyCommentId { get; set; }
}