using EveryDaily.Application.Dtos.Follow;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerQueries.Follow.Queries
{
    public class GetUserFollowStatsQuery : IRequest<Response<UserFollowStatsResponse>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserFollowStatsQueryHandler(AppDbContext context,IUserService userService)
        : IRequestHandler<GetUserFollowStatsQuery, Response<UserFollowStatsResponse>>
    {
        public async Task<Response<UserFollowStatsResponse>> Handle(GetUserFollowStatsQuery request, CancellationToken cancellationToken)
        {
            var followersCount = await context.Follows.CountAsync(f => f.FollowingId == request.UserId, cancellationToken);
            var followingCount = await context.Follows.CountAsync(f => f.FollowerId == request.UserId, cancellationToken);

            var stats = new UserFollowStatsResponse
            {
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };

            return Response<UserFollowStatsResponse>.Success(stats, 200);
        }
    }

}
