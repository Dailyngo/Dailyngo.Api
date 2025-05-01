using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class LikeDoc : DocBase
{
    public string UserId { get; set; }
    public ObjectId PostId { get; set; }
}