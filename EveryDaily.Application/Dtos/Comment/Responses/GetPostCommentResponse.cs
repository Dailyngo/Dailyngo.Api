using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Comment.Responses;

public class GetPostCommentResponse
{
    public ObjectId Id { get; set; }
    public ObjectId? ReplyCommentId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public bool CanDelete { get; set; }
    public string Content { get; set; }
    public DateTimeOffset? CommentDate { get; set; }
}