using EveryDaily.Core.Entity;
using MongoDB.Bson;

namespace EveryDaily.Domain.Documents;

public class TestModel : IEntityBase<ObjectId>
{
    public string TestName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public ObjectId Id { get; set; }
}
