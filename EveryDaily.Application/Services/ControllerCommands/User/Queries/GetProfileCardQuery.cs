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
using MongoDB.Bson;
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
            var currentUserId = userService.GetUserId();
            var profileUserId = request.UserId ?? currentUserId;

            var user = await appDbContext.Users
                .Select(x => new { x.Id, x.FullName, x.UserName, x.About.Bio })
                .FirstOrDefaultAsync(x => x.Id == profileUserId, cancellationToken);

            if (user == null)
            {
                return Response<GetProfileCardResponse>.Fail(UserErrorMessage.ProfileDetailNotFound);
            }

            var followersCount = await appDbContext.Follows.CountAsync(f => f.FollowingId == profileUserId, cancellationToken);
            var followingCount = await appDbContext.Follows.CountAsync(f => f.FollowerId == profileUserId, cancellationToken);

            var postCount = await mongoDocContext.Posts.Collection.CountDocumentsAsync(
                Builders<PostDoc>.Filter.And(
                    Builders<PostDoc>.Filter.Eq(x => x.UserId, profileUserId.ToString()),
                    Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false)
                ),
                cancellationToken: cancellationToken
            );

            // 1. Ben onu takip ediyor muyum?
            var isFollowing = await appDbContext.Follows.AnyAsync(f =>
                f.FollowerId == currentUserId && f.FollowingId == profileUserId, cancellationToken);

            // 2. O beni takip ediyor mu?
            var isFollowed = await appDbContext.Follows.AnyAsync(f =>
                f.FollowerId == profileUserId && f.FollowingId == currentUserId, cancellationToken);

            // 3. Ben ona takip isteği atmış mıyım? (HENÜZ O KABUL ETMEMİŞ)
            var sentFollowRequest = await mongoDocContext.FollowRequests.Collection
                .Find(f =>
                    f.SenderId == currentUserId.ToString() &&
                    f.ReceiverId == profileUserId.ToString()
                    && !f.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            // 4. O bana takip isteği atmış ama ben cevaplamamış mıyım?
            var receivedFollowRequest = await mongoDocContext.FollowRequests.Collection
                .Find(f =>
                    f.SenderId == profileUserId.ToString() &&
                    f.ReceiverId == currentUserId.ToString()
                    && !f.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

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
                },
                IsFollowing = isFollowing,
                IsFollowed = isFollowed,
                IsSendFollowRequest = sentFollowRequest != null,
                SendFollowRequestId = sentFollowRequest?.Id.ToString(),
                IsReceiverFollowRequest = receivedFollowRequest != null,
                SendReceiverRequestId = receivedFollowRequest?.Id.ToString()
            };

            return Response<GetProfileCardResponse>.Success(response);
        }
    }

}
