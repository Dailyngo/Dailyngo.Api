namespace EveryDaily.Application.Dtos.Notification
{
    public class FollowNotificationDto
    {
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string RelatedEntityId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
