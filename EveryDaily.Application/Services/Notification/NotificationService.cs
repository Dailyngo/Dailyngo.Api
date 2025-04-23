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
                IsDeleted = false
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
    }
}
