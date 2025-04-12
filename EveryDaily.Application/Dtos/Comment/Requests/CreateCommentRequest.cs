using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Comment.Requests;

public class CreateCommentRequest 
{
    public string Content { get; set; }
    public ObjectId PostId { get; set; }
    public ObjectId? ReplyCommentId { get; set; }
}