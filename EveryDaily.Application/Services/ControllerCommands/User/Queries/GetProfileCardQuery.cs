using EveryDaily.Application.Dtos.User.Response;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Prefix.ErrorMessage;
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
        public async Task<Response<GetProfileCardResponse>> Handle(GetProfileCardQuery request,
            CancellationToken cancellationToken)
        {
            var userID = userService.GetUserId();

            var user = await appDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userID, cancellationToken);

            if (user == null)
            {
                return Response<GetProfileCardResponse>.Fail(UserErrorMessage.ProfileDetailNotFound);
            }

            var response = new GetProfileCardResponse
            {
                Follower = 244,
                FollowUp = 300,
                PostCount = 120,
                GetUserResponse = new GetUserResponse
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                }
            };


            return Response<GetProfileCardResponse>.Success(response);
        }
    }
}