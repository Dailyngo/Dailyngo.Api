using EveryDaily.Application.Dtos.Follow;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerQueries.Follow.Queries
{
    public class GetUserFollowListQuery : IRequest<Response<List<UserFollowResponse>>>
    {
        public Guid UserId { get; set; }
        public bool IsFollowingList { get; set; } // true: takip ettikleri, false: takipçileri
        public int PageNumber { get; set; } = 1;

    }

    public class GetUserFollowListQueryHandler(AppDbContext context)
        : IRequestHandler<GetUserFollowListQuery, Response<List<UserFollowResponse>>>
    {
        public async Task<Response<List<UserFollowResponse>>> Handle(GetUserFollowListQuery request, CancellationToken cancellationToken)
        {

            int pageSize = 20;
            int skip = (request.PageNumber - 1) * pageSize;

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
                        UserId = f.FollowerId
                    })
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }

            return Response<List<UserFollowResponse>>.Success(followList, 200);
        }
    }

}
