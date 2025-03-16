using EveryDaily.Application.Dtos.Notification;
using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace EveryDaily.Application.Services.ControllerCommands.Notication.Queries
{
    public class GetNotificationsQuery : IRequest<Response<NotificationResponse>>
    {
    }
    public class GetNotificationsQueryHandler(
        IRedisService redisService,
        NotificationRepository notificationRepository,
        IUserService userService,
        AppDbContext appDbContext)
        : IRequestHandler<GetNotificationsQuery, Response<NotificationResponse>>
    {
        public async Task<Response<NotificationResponse>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            List<NotificationEntity> notifications = new();

            var redisKey = RedisPrefix.GetUserNotificationsKey(userId);
            var redisNotifications = await redisService.ListRangeAsync(redisKey);

            if (redisNotifications == null || redisNotifications.Length == 0)
            {
                notifications = await notificationRepository
                    .GetManyAsync(n => n.ReceiverId == userId && !n.IsRead && !n.IsDeleted);
            }
            else
            {
                notifications = redisNotifications
                    .Select(n => JsonConvert.DeserializeObject<NotificationEntity>(n))
                    .Where(n => n != null)
                    .ToList();
            }

            var followSenderIds = notifications
                .Where(n => n.Type == NotificationType.Follow)
                .Select(n => n.SenderId)
                .Distinct()
                .ToList();

            var senderNames = await appDbContext.Users.Where(u=> followSenderIds.Contains(u.Id)).Select(u => new { u.FullName , u.Id }).ToListAsync();   

            var groupedNotifications = new NotificationResponse
            {
                FollowRequests = notifications
                    .Where(n => n.Type == NotificationType.Follow)
                    .Select(n => new FollowNotificationDto
                    {
                        SenderId = n.SenderId,
                        SenderName = senderNames.Where(sn=> sn.Id == n.SenderId).Select(sn=> sn.FullName).First(),
                        RelatedEntityId = n.RelatedEntityId
                    })
                    .ToList()
            };

            if (notifications.Any())
            {
                var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);
                await notificationRepository.UpdateManyAsync(n => n.ReceiverId == userId && !n.IsRead, update);
            }

            await redisService.DeleteAsync(redisKey);

            return Response<NotificationResponse>.Success(groupedNotifications, 200);
        }
    }
}
