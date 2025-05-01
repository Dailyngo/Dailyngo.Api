using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents.Post;

public class ReportDoc : DocBase
{
    public ObjectId PostId { get; set; }
    public string UserId { get; set; }
    public string? ReportReason { get; set; }
    public bool IsProcess { get; set; }
}