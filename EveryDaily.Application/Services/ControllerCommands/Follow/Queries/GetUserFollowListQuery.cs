using EveryDaily.Application.Dtos.Follow;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Follow.Queries
{
    public class GetUserFollowListQuery : IRequest<Response<List<UserFollowResponse>>>
    {
        public Guid? UserId { get; set; }
        public bool IsFollowingList { get; set; } // true: takip ettikleri, false: takipçileri
        public int PageNumber { get; set; } = 1;
    }

    public class GetUserFollowListQueryHandler(AppDbContext context, IUserService userService)
        : IRequestHandler<GetUserFollowListQuery, Response<List<UserFollowResponse>>>
    {
        public async Task<Response<List<UserFollowResponse>>> Handle(GetUserFollowListQuery request,
            CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();
            request.UserId ??= userId;

            var pageSize = 40;
            var skip = (request.PageNumber - 1) * pageSize;

            var userFollowTable = await context.Follows
                .Where(x => x.FollowingId == userId || x.FollowerId == userId)
                .Select(s => new
                {
                    s.FollowerId,
                    s.FollowingId
                }).ToListAsync(cancellationToken);

            List<UserFollowResponse> followList;

            if (request.IsFollowingList)
            {
                // Kullanıcının takip ettiklerini getir (Following)
                followList = await context.Follows
                    .Where(f => f.FollowerId == request.UserId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new UserFollowResponse
                    {
                        FullName = f.Following.FullName,
                        UserName = f.Following.UserName,
                        UserId = f.FollowingId
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                // Kullanıcıyı takip edenleri getir (Followers)
                followList = await context.Follows
                    .Where(f => f.FollowingId == request.UserId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new UserFollowResponse
                    {
                        FullName = f.Follower.FullName,
                        UserName = f.Follower.UserName,
                        UserId = f.FollowerId
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }
            
            followList.ForEach(item =>
            {
                var userFollow = userFollowTable
                    .FirstOrDefault(x => x.FollowerId == item.UserId || x.FollowingId == item.UserId);
                if (userFollow != null)
                {
                    item.IsFollowing = userFollow.FollowerId == item.UserId;
                    item.IsFollower = userFollow.FollowingId == item.UserId;
                }
            });
            
            followList = followList.OrderByDescending(x => x.IsFollowing)
                .ThenByDescending(x => x.IsFollower)
                .ToList();

            return Response<List<UserFollowResponse>>.Success(followList, 200);
        }
    }
}