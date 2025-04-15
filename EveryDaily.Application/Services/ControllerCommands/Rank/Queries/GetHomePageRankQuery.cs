using EveryDaily.Application.Dtos.Rank;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.Rank.Queries
{
    public class GetHomePageRanksQuery : IRequest<Response<List<HomePageRankResponse>>>
    {
        public int PageNumber { get; set; }
    }

    public class GetHomePageRanksQueryHandler(AppDbContext dbContext, IUserService userService) : IRequestHandler<GetHomePageRanksQuery, Response<List<HomePageRankResponse>>>
    {
        public async Task<Response<List<HomePageRankResponse>>> Handle(GetHomePageRanksQuery request, CancellationToken cancellationToken)
        {
            var pagesize = 10; // Sayfa boyutu, her sayfada kaç sonuç gösterileceği
            var userId = userService.GetUserId();

            // Sezon belirlenmediyse en güncel sezonu al
            int seasonToUse = await dbContext.UserXpStatuses
                .OrderByDescending(r => r.Season)
                .Select(r => r.Season)
                .FirstOrDefaultAsync(cancellationToken);

            // Takip edilen kullanıcıların ID’lerini al
            var followedUserIds = await dbContext.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync(cancellationToken);

            // Takip edilenlerin sezon rank bilgilerini getir
            var rankList = await dbContext.UserXpStatuses
                .Include(r => r.User)
                .Where(r => r.Season == seasonToUse && followedUserIds.Contains(r.UserId))
                .OrderByDescending(r => r.Rank)
                .Skip((request.PageNumber-1) * pagesize)
                .Take(pagesize)
                .Select(r => new HomePageRankResponse
                {
                    UserId = r.UserId,
                    FullName = r.User.FullName,
                    Rank = r.Rank,
                    Season = r.Season
                })
                .ToListAsync(cancellationToken);

            return Response<List<HomePageRankResponse>>.Success(rankList, 200);
        }
    }
}
