using EveryDaily.Core.Entity;
using EveryDaily.Domain.Enums.Notification;
using MongoDB.Bson;

namespace EveryDaily.Domain.Entities.Notification
{
    public class NotificationEntity : IEntityBase<ObjectId>
    {
        public ObjectId Id { get; set; }
        public string ReceiverId { get; set; }
        public string SenderId { get; set; }
        public string? RelatedEntityId { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
