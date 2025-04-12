using EveryDaily.Application.Dtos.Post.Responses;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Queries;

public class GetHomePagePostQuery : IRequest<Response<List<GetUserPostResponse>>>
{
    public int PageNumber { get; set; }
}

public class GetHomePagePostQueryHandler(
    MongoDocContext mongoDocContext,
    AppDbContext appDbContext,
    IUserService userService)
    : IRequestHandler<GetHomePagePostQuery, Response<List<GetUserPostResponse>>>
{
    public async Task<Response<List<GetUserPostResponse>>> Handle(GetHomePagePostQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var recentThreshold = now.AddDays(-7);

        var userId = userService.GetUserId();

        int pageSize = 20;
        int skip = (request.PageNumber - 1) * pageSize;

        var followingUserIds = await appDbContext.Follows
            .Where(f => f.FollowerId == userId && !f.IsDeleted)
            .Select(f => f.FollowingId)
            .ToListAsync(cancellationToken);

        var filter = Builders<PostDoc>.Filter.And(
            Builders<PostDoc>.Filter.Eq(p => p.IsDeleted, false),
            Builders<PostDoc>.Filter.Or(
                Builders<PostDoc>.Filter.In(p => p.UserId, followingUserIds),
                Builders<PostDoc>.Filter.Gte(p => p.CreatedAt, recentThreshold)
            )
        );

        var filteredPosts = await mongoDocContext.Posts.Collection
            .Find(filter)
            .ToListAsync(cancellationToken); // pagination'ı skor sonrasına bırakacağımız için şimdilik tümünü çekiyoruz

        var scoredPosts = filteredPosts.Select(post =>
            {
                var isRecent = post.CreatedAt.HasValue && post.CreatedAt.Value >= recentThreshold;
                var isFromFollowedUser = followingUserIds.Contains(post.UserId);

                var score = (post.LikeCount * 2)
                            + (post.CommentCount * 3)
                            + (isRecent ? 7 : 0)
                            + (isFromFollowedUser ? 13 : 0)
                            + (post.ViewCount * 1);

                return new
                {
                    Post = post,
                    Score = score
                };
            })
            .OrderByDescending(x => x.Score)
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        var userIds = scoredPosts.Select(x => x.Post.UserId).Distinct();
        var userNames = await appDbContext.Users
            .Where(x => userIds.Contains(x.Id))
            .Select(x => new { x.Id, x.UserName })
            .ToListAsync(cancellationToken);

        var response = scoredPosts.Select(x =>
        {
            var userName = userNames.FirstOrDefault(u => u.Id == x.Post.UserId)?.UserName ?? "";

            return new GetUserPostResponse
            {
                Id = x.Post.Id.ToString(),
                UserId = x.Post.UserId,
                UserName = userName,
                Content = x.Post.Content,
                PostDate = x.Post.CreatedAt,
                LikeCount = x.Post.LikeCount,
                CommentCount = x.Post.CommentCount,
            };
        }).ToList();

        return Response<List<GetUserPostResponse>>.Success(response);
    }
}