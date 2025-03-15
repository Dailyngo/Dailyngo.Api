using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Application.Dtos.User.Response;
using EveryDaily.Application.Services.ControllerCommands.About.Queries;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Application.Services.ControllerCommands.User.Queries
{
    public class GetProfileCardQuery : IRequest<Response<GetProfileCardResponse>>
    {

    }

    public class GetProfileCardQueryHandler(AppDbContext appDbContext, IUserService userService)
        : IRequestHandler<GetProfileCardQuery, Response<GetProfileCardResponse>>
    {
        public async Task<Response<GetProfileCardResponse>> Handle(GetProfileCardQuery request, CancellationToken cancellationToken)
        {

            var userID = userService.GetUserId();

            var profileCard = await appDbContext.ProfileCards
                .Include(i => i.User)
                .FirstOrDefaultAsync(x => x.UserId == userID, cancellationToken);


            var response = new GetProfileCardResponse
            {
                Follower = profileCard.Follower,
                FollowUp = profileCard.FollowUp,
                PostCount = profileCard.PostCount,
                GetUserResponse = new GetUserResponse(){
                    FullName = profileCard.User.FullName,
                }
            };





            return Response<GetProfileCardResponse>.Success(response);

        }
    }
}
