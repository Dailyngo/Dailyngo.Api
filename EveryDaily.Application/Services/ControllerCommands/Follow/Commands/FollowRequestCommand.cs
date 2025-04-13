using EveryDaily.Application.Services.UserService;
using EveryDaily.Application.Socket;
using EveryDaily.Core;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Commands
{
    public class FollowRequestCommand : IRequest<Response<NoContent>>
    {
        public Guid ReceiverId { get; set; }
    }

    public class FollowRequestCommandHandler(
        IRedisService redisService,
        IHubContext<NotificationHub> hubContext,
        MongoDocContext mongoDocContext,
        IUserService userService,
        AppDbContext context
        )
        : IRequestHandler<FollowRequestCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(FollowRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            if (userId == request.ReceiverId)
            {
                return Response<NoContent>.Fail("Kendini takip etmek istiyorsan aynaya bak :)");
            }

            var isAlreadyFollowing = await userService.IsFollowingAsync(request.ReceiverId, context);
            if (isAlreadyFollowing)
            {
                return Response<NoContent>.Fail("Bu kullanıcıyı zaten takip ediyorsun.");
            }


            var existingRequest = await mongoDocContext.FollowRequests.Collection.Find(
                f => f.SenderId == userId.ToString()
                     && f.ReceiverId == request.ReceiverId.ToString()).AnyAsync(cancellationToken);

            if (existingRequest)
            {
                return Response<NoContent>.Fail("Zaten bir takip isteği göndermişsin. Sen ne olsun istiyorsun ?");
            }
           

            var followRequest = new FollowRequestEntity
            {
                SenderId = userId.ToString(),
                ReceiverId = request.ReceiverId.ToString()
            };

            await mongoDocContext.FollowRequests.Collection.InsertOneAsync(followRequest, new(), cancellationToken);

            var notification = new NotificationEntity
            {
                ReceiverId = request.ReceiverId.ToString(),
                SenderId = userId.ToString(),
                Type = NotificationType.Follow,
                RelatedEntityId = followRequest.Id.ToString(),
                IsDeleted = false
            };

            await mongoDocContext.Notifications.Collection.InsertOneAsync(notification, new(), cancellationToken);

            await redisService.ListLeftPushAsync(
                RedisPrefix.GetUserNotificationsKey(request.ReceiverId),
                $"{NotificationType.Follow}", TimeSpan.FromDays(1)
            );

            await hubContext.Clients.Group(request.ReceiverId.ToString()).SendAsync(
                NotificationHubMethods.ReceiveNotification,
                await mongoDocContext.Notifications.Collection.CountDocumentsAsync(
                    n => n.ReceiverId == request.ReceiverId.ToString() && !n.IsRead, new(), cancellationToken),
                cancellationToken
            );


            return Response<NoContent>.Success(201);
        }
    }
}