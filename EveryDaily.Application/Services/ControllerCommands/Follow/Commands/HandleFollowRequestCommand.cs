using EveryDaily.Application.Services.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Commands
{
    public class HandleFollowRequestCommand : IRequest<Response<NoContent>>
    {
        public string RequestId { get; set; }
        public bool IsAccepted { get; set; } // true: kabul, false: reddet
    }

    public class HandleFollowRequestCommandHandler(
        AppDbContext context,
        MongoDocContext mongoDocContext,
        IUserService userService,
        INotificationService notificationService)
        : IRequestHandler<HandleFollowRequestCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(HandleFollowRequestCommand request,
            CancellationToken cancellationToken)
        {
            var receiverId = userService.GetUserId();
            var followRequest = await mongoDocContext.FollowRequests.Collection
                .Find(f => f.Id == new ObjectId(request.RequestId)).FirstOrDefaultAsync(cancellationToken);

            if (followRequest == null || followRequest.IsDeleted)
                return Response<NoContent>.Fail("Hiç bir şey vardan yok yoktan var olamaz :)");

            if (followRequest.ReceiverId != receiverId.ToString())
            {
                return Response<NoContent>.Fail("Sen bana ordan bi touch blue alır mısın ?");
            }

            var followEntity = new FollowEntity
            {
                FollowerId = Guid.Parse(followRequest.SenderId),
                FollowingId = Guid.Parse(followRequest.ReceiverId)
            };
            if (request.IsAccepted)
            {
                await context.Follows.AddAsync(followEntity, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            await notificationService.RemoveFollowRequestNotificationAsync(
            receiverId: followRequest.ReceiverId,
            senderId: followRequest.SenderId,
            relatedEntityId: followRequest.Id.ToString(), 
            cancellationToken);

            await mongoDocContext.FollowRequests.Collection.DeleteOneAsync(f => f.Id == followRequest.Id,
                new DeleteOptions(), cancellationToken);


            return Response<NoContent>.Success(200);
        }
    }
}