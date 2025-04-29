using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class ReportDoc : IEntityBase<ObjectId>
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ObjectId Id { get; set; }
    public ObjectId PostId { get; set; }
    public string UserId { get; set; }
    public string? ReportReason { get; set; }
    public bool IsProcess { get; set; }
}