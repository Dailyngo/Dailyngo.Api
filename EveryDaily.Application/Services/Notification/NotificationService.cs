using EveryDaily.Application.Socket;
using EveryDaily.Core;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence.MongoContext;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;


namespace EveryDaily.Application.Services.Notification
{
    public class NotificationService(MongoDocContext mongoDocContext
        ,IRedisService redisService
        , IHubContext<NotificationHub> hubContext) : INotificationService
    {

        public async Task SendNotification(string receiverId,string userId,string relatedEntityId, NotificationType type,CancellationToken cancellationToken)
        {
            var notification = new NotificationEntity
            {
                ReceiverId = receiverId,
                SenderId = userId,
                Type = type,
                RelatedEntityId = relatedEntityId,
                IsDeleted = false,
                CreatedAt
                = DateTime.UtcNow,
            };

            await mongoDocContext.Notifications.Collection.InsertOneAsync(notification, new(), cancellationToken);

            await redisService.ListLeftPushAsync(
                RedisPrefix.GetUserNotificationsKey(Guid.Parse(receiverId)),
                $"{type}", TimeSpan.FromDays(1)
            );

            await hubContext.Clients.Group(receiverId).SendAsync(
                NotificationHubMethods.ReceiveNotification,
                await mongoDocContext.Notifications.Collection.CountDocumentsAsync(
                    n => n.ReceiverId == receiverId && !n.IsRead, new(), cancellationToken),
                cancellationToken
            );
        }

        public async Task RemoveFollowRequestNotificationAsync(string receiverId, string senderId, string? relatedEntityId = null, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<NotificationEntity>.Filter;

            var filter = filterBuilder.And(
                filterBuilder.Eq(n => n.ReceiverId, receiverId),
                filterBuilder.Eq(n => n.SenderId, senderId),
                filterBuilder.Eq(n => n.Type, NotificationType.Follow)
            );

            if (!string.IsNullOrWhiteSpace(relatedEntityId))
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.Eq(n => n.RelatedEntityId, relatedEntityId)
                );
            }

            // Bildirimi tamamen sil
            await mongoDocContext.Notifications.Collection.DeleteManyAsync(filter, cancellationToken);

            // Bildirim sayısını yeniden hesapla ve gönder
            var unreadCount = await mongoDocContext.Notifications.Collection.CountDocumentsAsync(
                n => n.ReceiverId == receiverId && !n.IsRead,
                cancellationToken: cancellationToken);

            await hubContext.Clients.Group(receiverId).SendAsync(
                NotificationHubMethods.ReceiveNotification,
                unreadCount,
                cancellationToken);
        }

    }



}
