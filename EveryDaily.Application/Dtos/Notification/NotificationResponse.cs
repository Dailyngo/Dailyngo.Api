using EveryDaily.Domain.Enums.Notification;

namespace EveryDaily.Application.Dtos.Notification
{
    public class NotificationResponse
    {
        public List<FollowNotificationDto> FollowRequests { get; set; } = new();
        public List<BaseNotificationDto> OtherNotifications { get; set; } = new();
    }

    public class BaseNotificationDto
    {
        public Guid SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? Text { get; set; } // CommentText, AnnouncementMessage gibi
        public NotificationType NotificationType { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
