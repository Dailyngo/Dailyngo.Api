using EveryDaily.Application.Dtos.Rank;
using EveryDaily.Application.Services.Badge;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;

namespace EveryDaily.Application.Services.ControllerCommands.Rank.Queries
{
    public class GetUserRankQuery : IRequest<Response<List<UserRankResponse>>>
    {
        public Guid? UserId { get; set; }
        public bool Old { get; set; }  // Tüm sezonları almak için kullanılan parametre
    }

    public class GetUserRankQueryHandler(AppDbContext dbContext, IUserService userService,IRankService rankService) : IRequestHandler<GetUserRankQuery, Response<List<UserRankResponse>>>
    {
        public async Task<Response<List<UserRankResponse>>> Handle(GetUserRankQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId ?? userService.GetUserId();

            var userRankDtos = await rankService.GetUserRankAsync(userId, request.Old, cancellationToken);

            return Response<List<UserRankResponse>>.Success(userRankDtos, 200);
        }
    }
}
