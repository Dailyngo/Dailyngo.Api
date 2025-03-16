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

    public class UnfollowUserCommandHandler(AppDbContext context,IUserService userService)
        : IRequestHandler<UnfollowUserCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            if (request.IsRemovingFollower)
            {
                // Beni takip eden kişiyi kaldır
                var follower = await context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == request.UserId && f.FollowingId == userId, cancellationToken);

                if (follower == null)
                    return Response<NoContent>.Fail("Bu kullanıcı sizi takip etmiyor.", 404);

                context.Follows.Remove(follower);
            }
            else
            {
                // Takip ettiğim kişiyi kaldır
                var following = await context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == request.UserId, cancellationToken);

                if (following == null)
                    return Response<NoContent>.Fail("Bu kullanıcıyı takip etmiyorsunuz.", 404);

                context.Follows.Remove(following);
            }

            await context.SaveChangesAsync(cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
