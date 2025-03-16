using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Fallow;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Persistence;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Commands
{
    public class HandleFollowRequestCommand : IRequest<Response<NoContent>>
    {
        public string RequestId { get; set; }
        public bool IsAccepted { get; set; } // true: kabul, false: reddet
    }

    public class HandleFollowRequestCommandHandler(AppDbContext context, FollowRequestRepository followRequestRepository,IUserService userService)
        : IRequestHandler<HandleFollowRequestCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(HandleFollowRequestCommand request, CancellationToken cancellationToken)
        {
            var receiverId = userService.GetUserId();
            var followRequest = await followRequestRepository.GetByIdAsync(new MongoDB.Bson.ObjectId(request.RequestId));

            if (followRequest == null || !followRequest.IsDeleted)
                return Response<NoContent>.Fail("Hiç bir şey vardan yok yoktan var olamaz :)");

            if (followRequest.ReceiverId != receiverId){
                return Response<NoContent>.Fail("Sen bana ordan bi touch blue alır mısın ?");
            }

            var followEntity = new FollowEntity
            {
                FollowerId = followRequest.SenderId,  
                FollowingId = followRequest.ReceiverId 
            };

            await context.Follows.AddAsync(followEntity, cancellationToken);

            await followRequestRepository.DeleteAsync(followRequest.Id);

            await context.SaveChangesAsync(cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
