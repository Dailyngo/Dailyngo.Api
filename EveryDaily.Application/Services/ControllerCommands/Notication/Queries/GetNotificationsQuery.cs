using EveryDaily.Application.Dtos.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Application.Socket;
using EveryDaily.Core;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence.MongoContext;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using EveryDaily.Core.Dtos;
using Microsoft.EntityFrameworkCore;

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
            .Find(n => n.ReceiverId == userId.ToString())
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        var senderIds = notifications
            .Select(n => Guid.Parse(n.SenderId))
            .Distinct()
            .ToList();

        var senderNames = await appDbContext.Users
            .Where(u => senderIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var commentIds = notifications
            .Where(n => n.Type == NotificationType.Comment)
            .Select(n => ObjectId.Parse(n.RelatedEntityId))
            .ToList();

        var commentFilter = Builders<CommentDoc>.Filter.And(
            Builders<CommentDoc>.Filter.In(c => c.Id, commentIds),
            Builders<CommentDoc>.Filter.Eq(c => c.IsDeleted, false)
        );

        var comments = await mongoDocContext.Comments.Collection
            .Find(commentFilter)
            .ToListAsync(cancellationToken);

        var followRequests = notifications
            .Where(n => n.Type == NotificationType.Follow)
            .Select(n => new FollowNotificationDto
            {
                SenderId = Guid.Parse(n.SenderId),
                SenderName = senderNames.FirstOrDefault(sn => sn.Id == Guid.Parse(n.SenderId))?.FullName,
                RelatedEntityId = n.RelatedEntityId,
                CreatedAt = n.CreatedAt
            })
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        var otherNotifications = notifications
            .Where(n => n.Type != NotificationType.Follow)
            .Select(n =>
            {
                var senderId = Guid.Parse(n.SenderId);
                var senderName = senderNames.FirstOrDefault(sn => sn.Id == senderId)?.FullName;

                return new BaseNotificationDto
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    RelatedEntityId = n.Type == NotificationType.Comment
                        ? comments.FirstOrDefault(c => c.Id == ObjectId.Parse(n.RelatedEntityId))?.PostId.ToString()
                        : n.RelatedEntityId,
                    Text = n.Type == NotificationType.Comment
                        ? FormatComment(comments.FirstOrDefault(c => c.Id == ObjectId.Parse(n.RelatedEntityId)))
                        : null,
                    NotificationType = n.Type,
                    CreatedAt = n.CreatedAt
                };
            })
            .Where(n => n.SenderId != Guid.Empty)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        if (notifications.Any())
        {
            var update = Builders<NotificationEntity>.Update.Set(n => n.IsRead, true);
            await mongoDocContext.Notifications.Collection.UpdateManyAsync(
                n => n.ReceiverId == userId.ToString() && !n.IsRead, update, cancellationToken: cancellationToken);
        }

        await redisService.DeleteAsync(redisKey);

        await hubContext.Clients.Group(userId.ToString())
            .SendAsync(NotificationHubMethods.ReceiveNotification, 0, cancellationToken);

        return Response<NotificationResponse>.Success(new NotificationResponse
        {
            FollowRequests = followRequests,
            OtherNotifications = otherNotifications
        }, 200);
    }

    private string FormatComment(CommentDoc? comment)
    {
        if (comment == null || string.IsNullOrWhiteSpace(comment.Content))
            return null;

        return comment?.Content?.Length > 50
            ? comment.Content.Substring(0, 50) + "..."
            : comment?.Content ?? "";
    }
}
