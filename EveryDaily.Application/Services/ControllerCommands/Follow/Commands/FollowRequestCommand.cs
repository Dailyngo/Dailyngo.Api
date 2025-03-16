using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Application.Socket;
using EveryDaily.Core;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Fallow;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Commands
{
    public class FollowRequestCommand : IRequest<Response<NoContent>>
    {
        public Guid ReceiverId { get; set; }
    }

    public class FollowRequestCommandHandler(
        IRedisService redisService,
        IHubContext<NotificationHub> hubContext,
        NotificationRepository notificationRepository,
        FollowRequestRepository followRequestRepository,
        IUserService userService)
        : IRequestHandler<FollowRequestCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(FollowRequestCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            if (userId == request.ReceiverId)
            {
                return Response<NoContent>.Fail("Kendini takip etmek istiyorsan aynaya bak :)");
            }

            var existingRequest = await followRequestRepository.ExistsAsync(
                f => f.SenderId == userId.ToString() 
                && f.ReceiverId == request.ReceiverId.ToString());

            if (existingRequest)
            {
                return Response<NoContent>.Fail("Zaten bir takip isteği göndermişsin. Sen ne olsun istiyorsun ?");
            }

            var followRequest = new FollowRequestEntity
            {
                SenderId = userId.ToString(),
                ReceiverId = request.ReceiverId.ToString()
            };

            await followRequestRepository.InsertAsync(followRequest);

            var notification = new NotificationEntity
            {
                ReceiverId = request.ReceiverId.ToString(),
                SenderId = userId.ToString(),
                Type = NotificationType.Follow,
                RelatedEntityId = followRequest.Id.ToString(),
                IsDeleted = false
            };

            await notificationRepository.InsertOneAsync(notification, cancellationToken);

            await redisService.ListLeftPushAsync(
                RedisPrefix.GetUserNotificationsKey(request.ReceiverId),
                $"{NotificationType.Follow}",TimeSpan.FromDays(1)
            );

            await hubContext.Clients.Group(request.ReceiverId.ToString()).SendAsync(
                NotificationHubMethods.ReceiveNotification,
                await notificationRepository.CountDocumentsAsync(n => n.ReceiverId == request.ReceiverId.ToString() && !n.IsRead),
                cancellationToken
            );


            return Response<NoContent>.Success(201);
        }
    }

}
