using EveryDaily.Application.Services.Badge;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Enums.Rank;
using MediatR;

namespace EveryDaily.Application.Services.ControllerCommands.Rank.Commands
{
    public class LoginRankCommand : IRequest<Response<NoContent>>
    {
    }

    public class LoginRankCommandHandler : IRequestHandler<LoginRankCommand, Response<NoContent>>
    {
        private readonly IRankService rankService;
        private readonly IUserService userService;

        public LoginRankCommandHandler(IRankService rankService, IUserService userService)
        {
            this.rankService = rankService;
            this.userService = userService;
        }

        public async Task<Response<NoContent>> Handle(LoginRankCommand request, CancellationToken cancellationToken)
        {
            var userId = userService.GetUserId();

            await rankService.ProcessActivityAsync(userId, XpActivityType.login, cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
