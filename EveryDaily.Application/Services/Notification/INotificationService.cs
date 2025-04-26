using EveryDaily.Domain.Enums.Notification;

namespace EveryDaily.Application.Services.Notification
{
    public interface INotificationService
    {
        public Task SendNotification(string receiverId, string userId, string relatedEntityId, NotificationType type, CancellationToken cancellationToken);
        public Task RemoveFollowRequestNotificationAsync(string receiverId, string senderId, string? relatedEntityId = null, CancellationToken cancellationToken = default);
    }
}
