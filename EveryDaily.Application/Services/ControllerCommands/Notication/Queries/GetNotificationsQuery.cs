using EveryDaily.Application.Dtos.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Notication.Queries
{
    public class GetNotificationsQuery : IRequest<Response<NotificationResponse>>
    {
    }

    public class GetNotificationsQueryHandler(
        IRedisService redisService,
        MongoDocContext mongoDocContext,
        IUserService userService,
        AppDbContext appDbContext)
        : IRequestHandler<GetNotificationsQuery, Response<NotificationResponse>>
    {
        public async Task<Response<NotificationResponse>> Handle(GetNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();
            var redisKey = RedisPrefix.GetUserNotificationsKey(userId);

            var notifications = await mongoDocContext.Notifications.Collection
                    .Find(n => n.ReceiverId == userId.ToString()).ToListAsync(cancellationToken)
                ;

            var senderNames = await appDbContext.Users
                .Where(u => notifications.Select(x => Guid.Parse(x.SenderId)).Contains(u.Id))
                .Select(u => new { u.FullName, u.Id }).ToListAsync(cancellationToken);

            var groupedNotifications = new NotificationResponse
            {
                FollowRequests = notifications
                    .Where(n => n.Type == NotificationType.Follow)
                    .Select(n => new FollowNotificationDto
                    {
                        SenderId = Guid.Parse(n.SenderId),
                        SenderName = senderNames.Where(sn => sn.Id == Guid.Parse(n.SenderId)).Select(sn => sn.FullName)
                            .First(),
                        RelatedEntityId = n.RelatedEntityId
                    })
                    .ToList()
            };

            if (notifications.Any())
            {
                var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);
                await mongoDocContext.Notifications.Collection.UpdateManyAsync(
                    n => n.ReceiverId == userId.ToString() && !n.IsRead, update);
            }

            await redisService.DeleteAsync(redisKey);

            return Response<NotificationResponse>.Success(groupedNotifications, 200);
        }
    }
}