using EveryDaily.Core.Entity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class PostDoc : IEntityBase<ObjectId>
{
    public Guid UserId { get; set; }
    public string Content { get; set; }
    public string? ImageUrl { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ObjectId Id { get; set; }
}