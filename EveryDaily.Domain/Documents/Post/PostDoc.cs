using EveryDaily.Core.Entity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class PostDoc : DocBase
{
    public string UserId { get; set; }
    public string Content { get; set; }
    public string? ImageUrl { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
}