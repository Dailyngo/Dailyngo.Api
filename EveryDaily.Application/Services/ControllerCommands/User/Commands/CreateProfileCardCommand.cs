using EveryDaily.Application.Dtos.User.Request;
using EveryDaily.Application.Services.ControllerCommands.About.Commands;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using MediatR;

namespace EveryDaily.Application.Services.ControllerCommands.User.Commands
{
    public class CreateProfileCardCommand : IRequest<Response<NoContent>>
    {

        public CreateProfileCardRequest Data { get; set; }
    }

    public class CreateProfileCardHandler(AppDbContext appDbContext, IUserService userService)
        : IRequestHandler<CreateProfileCardCommand, Response<NoContent>>
    {
        public async Task<Response<NoContent>> Handle(CreateProfileCardCommand request, CancellationToken cancellationToken)
        {
            var userID = userService.GetUserId();
            var email = userService.GetUserEmail();

            var profileCardEntity = new ProfileCardEntity
            {
                UserId = userID,
                Follower=0,
                FollowUp = 0,
                PostCount = 0,

            };

            await appDbContext.AddAsync(profileCardEntity, cancellationToken);
            await appDbContext.SaveChangesAsync(cancellationToken);

            return Response<NoContent>.Success(200);
        }
    }
}
