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
    }

    public class GetUserFollowListQueryHandler(AppDbContext context)
        : IRequestHandler<GetUserFollowListQuery, Response<List<UserFollowResponse>>>
    {
        public async Task<Response<List<UserFollowResponse>>> Handle(GetUserFollowListQuery request, CancellationToken cancellationToken)
        {
            List<UserFollowResponse> followList;

            if (request.IsFollowingList)
            {
                // Kullanıcının takip ettiklerini getir (Following)
                followList = await context.Follows
                    .Where(f => f.FollowerId == request.UserId)
                    .Select(f=> new UserFollowResponse
                    {
                        FullName = f.Following.FullName,
                        UserId = f.FollowingId
                    })
                    .ToListAsync(cancellationToken);
            }
            else
            {
                // Kullanıcıyı takip edenleri getir (Followers)
                followList = await context.Follows
                    .Where(f => f.FollowingId == request.UserId)
                    .Select(f=> new UserFollowResponse
                    {
                        FullName = f.Follower.FullName,
                        UserId = f.FollowerId
                    })
                    .ToListAsync(cancellationToken);
            }

            return Response<List<UserFollowResponse>>.Success(followList, 200);
        }
    }

}
