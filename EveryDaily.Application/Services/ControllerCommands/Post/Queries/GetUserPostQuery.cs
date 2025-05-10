using EveryDaily.Application.Dtos.Post.Responses;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Queries;

public class GetUserPostQuery : IRequest<Response<List<GetUserPostResponse>>>
{
    public Guid? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
}

public class GetUserPostQueryHandler(
    IUserService userService,
    AppDbContext appDbContext,
    MongoDocContext mongoDocContext)
    : IRequestHandler<GetUserPostQuery, Response<List<GetUserPostResponse>>>
{
    public async Task<Response<List<GetUserPostResponse>>> Handle(GetUserPostQuery request,
        CancellationToken cancellationToken)
    {
        var userName = "";
        var userId = userService.GetUserId();
        if (request.UserId.HasValue)
        {
            // todo user takip ediyor mu ya da hesabı gizli değilmi kontrolu yapılacak
            // var user = await appDbContext
            //     .Follows
            //     .Any(x => x.FollowerId == userId && x.FollowingId == request.UserId.Value);

            var userExist = await appDbContext.Users
                .Where(x => x.Id == request.UserId.Value && !x.IsDeleted)
                .Select(x => new
                {
                    x.UserName
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (userExist == null)
                return Response<List<GetUserPostResponse>>.Success(204);

            userName = userExist.UserName;
        }
        else
        {
            var userExist = await appDbContext.Users
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => new
                {
                    x.UserName
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (userExist == null)
                return Response<List<GetUserPostResponse>>.Success(204);

            userName = userExist.UserName;
            request.UserId = userId;
        }

        var pageSize = 10;
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var skip = (pageNumber - 1) * pageSize;

        var filter = Builders<PostDoc>.Filter.And(
            Builders<PostDoc>.Filter.Eq(x => x.UserId, request.UserId.Value.ToString()),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var postsCursor = await mongoDocContext.Posts.Collection
            .Find(filter)
            .Skip(skip)
            .Limit(pageSize)
            .ToCursorAsync(cancellationToken: cancellationToken);

        var postList = await postsCursor.ToListAsync(cancellationToken: cancellationToken);

        var likeFilter = Builders<LikeDoc>.Filter.And(
            Builders<LikeDoc>.Filter.Eq(x => x.IsDeleted, false),
            Builders<LikeDoc>.Filter.In(p => p.PostId, postList.Select(x => x.Id)));

        var likeCursor = await mongoDocContext.Likes.Collection
            .Find(likeFilter)
            .ToCursorAsync(cancellationToken: cancellationToken);

        var likeList = await likeCursor.ToListAsync(cancellationToken: cancellationToken);

        var likeListGrouped = likeList.GroupBy(x => x.PostId)
            .Select(g => new
            {
                PostId = g.Key,
                LikeUserIds = g.Select(x => x.UserId).ToList()
            }).ToList();

        var response = postList.Select(x => new GetUserPostResponse
        {
            Id = x.Id.ToString(),
            UserName = userName,
            UserId = request.UserId ?? userService.GetUserId(),
            Content = x.Content,
            IsOwner = x.UserId == userId.ToString(),
            IsLiked = likeListGrouped
                .FirstOrDefault(l => l.PostId == x.Id)?.LikeUserIds.Contains(userService.GetUserId().ToString()) ?? false,
            PostDate = x.CreatedAt,
            LikeCount = x.LikeCount,
            CommentCount = x.CommentCount,
            ImageKey = x.ImageUrl
        }).ToList();

        return Response<List<GetUserPostResponse>>.Success(response);
    }
}