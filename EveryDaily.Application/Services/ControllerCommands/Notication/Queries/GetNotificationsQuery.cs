using EveryDaily.Application.Dtos.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Application.Socket;
using EveryDaily.Core;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
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
        IHubContext<NotificationHub> hubContext,
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


            var commentIds = notifications.Where(n => n.Type == NotificationType.Comment).Select(n =>ObjectId.Parse(n.RelatedEntityId));
            var filter = Builders<CommentDoc>.Filter.And(
            Builders<CommentDoc>.Filter.In(x => x.Id,commentIds),
            Builders<CommentDoc>.Filter.Eq(x => x.IsDeleted, false)
            );


            var comments = await mongoDocContext.Comments.Collection
           .Find(filter)

           .ToListAsync(cancellationToken);

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
                    .ToList(),

                CommentNotifications = notifications
                .Where(n => n.Type == NotificationType.Comment)
                .Select(n =>
                {
                    var senderIdGuid = Guid.Parse(n.SenderId);
                    var senderName = senderNames.First(sn => sn.Id == senderIdGuid).FullName;
                    var comment = comments.FirstOrDefault(c => c.Id == ObjectId.Parse(n.RelatedEntityId));

                    return new CommentNotificationDto
                    {
                        SenderId = senderIdGuid,
                        SenderName = senderName,
                        RelatedEntityId = comment.PostId.ToString(),
                        CommentText = comment == null
                            ? null
                            : comment.Content?.Length > 50
                                ? comment.Content.Substring(0, 50) + "..."
                                : comment.Content
                    };
                })
                .ToList(),


                Likes = notifications
                    .Where(n => n.Type == NotificationType.Like)
                    .Select(n => new LikeNotificationDto
                    {
                         SenderId = Guid.Parse(n.SenderId),
                         SenderName = senderNames.Where(sn => sn.Id == Guid.Parse(n.SenderId)).Select(sn => sn.FullName).FirstOrDefault(),
                         RelatedEntityId = n.RelatedEntityId
                    })
                .ToList(),

                Announcements = notifications
                    .Where(n => n.Type == NotificationType.Announcement)
                    .Select(n => new AnnouncementNotificationDto
                    {
                        // Message = n.Message,
                         RelatedEntityId = n.RelatedEntityId
                    })
                .ToList()

            };

            if (notifications.Any())
            {
                var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);
                await mongoDocContext.Notifications.Collection.UpdateManyAsync(
                    n => n.ReceiverId == userId.ToString() && !n.IsRead, update, cancellationToken: cancellationToken);
            }

            await redisService.DeleteAsync(redisKey);

            await hubContext.Clients.Group(userId.ToString())
                .SendAsync(NotificationHubMethods.ReceiveNotification, 0,cancellationToken);
            
            return Response<NotificationResponse>.Success(groupedNotifications, 200);
        }
    }
}