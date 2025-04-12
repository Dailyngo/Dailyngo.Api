using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Commands
{
    public class UnfollowUserCommand : IRequest<Response<NoContent>>
    {
        public Guid UserId { get; set; } // Takibi bırakılan kişi
        public bool IsRemovingFollower { get; set; }
    }

    public class UnfollowUserCommandHandler(AppDbContext context, IUserService userService)
        : IRequestHandler<UnfollowUserCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            if (request.IsRemovingFollower)
            {
                bool isFollowedBy = await userService.IsFollowedByAsync(request.UserId, context);
               
                if (!isFollowedBy)
                    return Response<NoContent>.Fail("Bu kullanıcı sizi takip etmiyor.", 404);

                // Beni takip eden kişiyi kaldır
                var follower = await context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == request.UserId && f.FollowingId == userId, cancellationToken);

                context.Follows.Remove(follower);
            }
            else
            {
                bool isFollowing = await userService.IsFollowingAsync(request.UserId, context);
                
                if (!isFollowing)
                    return Response<NoContent>.Fail("Bu kullanıcıyı takip etmiyorsunuz.", 404);

                // Takip ettiğim kişiyi kaldır
                var following = await context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == request.UserId, cancellationToken);

                context.Follows.Remove(following);
            }

            await context.SaveChangesAsync(cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
