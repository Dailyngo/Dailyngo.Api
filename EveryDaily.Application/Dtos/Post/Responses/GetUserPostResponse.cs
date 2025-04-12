using MongoDB.Bson;

namespace EveryDaily.Application.Dtos.Post.Responses;

public class GetUserPostResponse
{
    public ObjectId Id { get; set; }
    public string Content { get; set; }
    public DateTimeOffset? PostDate { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public bool IsLiked { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserProfileImage { get; set; }
}