using EveryDaily.Application.Dtos.Follow;
using EveryDaily.Application.Dtos.User.Response;
using EveryDaily.Application.Services.ControllerQueries.Follow.Queries;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.User.Queries
{
    public class GetProfileCardQuery : IRequest<Response<GetProfileCardResponse>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetProfileCardQueryHandler(
        AppDbContext appDbContext,
        IUserService userService,
        MongoDocContext mongoDocContext)
        : IRequestHandler<GetProfileCardQuery, Response<GetProfileCardResponse>>
    {
        public async Task<Response<GetProfileCardResponse>> Handle(GetProfileCardQuery request,
            CancellationToken cancellationToken)
        {
            var userID = request.UserId ?? userService.GetUserId();

            var user = await appDbContext.Users
                .Select(x => new { x.Id, x.FullName, x.UserName, x.About.Bio })
                .FirstOrDefaultAsync(x => x.Id == userID, cancellationToken);

            if (user == null)
            {
                return Response<GetProfileCardResponse>.Fail(UserErrorMessage.ProfileDetailNotFound);
            }

            var followersCount = await appDbContext.Follows.CountAsync(f => f.FollowingId == userID, cancellationToken);
            var followingCount = await appDbContext.Follows.CountAsync(f => f.FollowerId == userID, cancellationToken);

            var postCount = await mongoDocContext.Posts.Collection.CountDocumentsAsync(
                Builders<PostDoc>.Filter.And(
                    Builders<PostDoc>.Filter.Eq(x => x.UserId, userID.ToString()),
                    Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false)
                ),
                cancellationToken: cancellationToken
            );

            var response = new GetProfileCardResponse
            {
                PostCount = (int)postCount,
                Bio = user.Bio,
                Follower = followersCount,
                Following = followingCount,
                GetUserResponse = new GetUserResponse
                {
                    ProfilePicture = null,
                    FullName = user.FullName,
                    UserName = user.UserName,
                }
            };

            return Response<GetProfileCardResponse>.Success(response);
        }
    }
}
