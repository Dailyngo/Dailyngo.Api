using EveryDaily.Core.Entity;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EveryDaily.Domain.Enums.Fallow;

namespace EveryDaily.Domain.Entities.Follow
{
    public class FollowRequestEntity : IEntityBase<ObjectId>
    {
        public ObjectId Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? UpdatedAt { get ; set ; }
        public bool IsDeleted { get; set; }
        DateTimeOffset? IEntityBase.CreatedAt { get; set; }
    }
}
