using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Comment.Requests;

public class CreateCommentRequest 
{
    public string Content { get; set; }
    public string PostId { get; set; }
    public string? ReplyCommentId { get; set; }
}