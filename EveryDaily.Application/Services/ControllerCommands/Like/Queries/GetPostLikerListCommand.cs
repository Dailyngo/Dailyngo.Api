using EveryDaily.Application.Dtos.Like.Responses;
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

namespace EveryDaily.Application.Services.ControllerCommands.Like.Queries;

public class GetPostLikerListCommand : IRequest<Response<List<GetPostLikerResponse>>>
{
    public ObjectId PostId { get; set; }
    public int PageNumber { get; set; }
}

public class GetPostLikerListCommandHandler(
    MongoDocContext mongoDocContext,
    AppDbContext appDbContext,
    IUserService userService)
    : IRequestHandler<GetPostLikerListCommand, Response<List<GetPostLikerResponse>>>
{
    public async Task<Response<List<GetPostLikerResponse>>> Handle(GetPostLikerListCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        int pageSize = 20;
        int skip = (request.PageNumber - 1) * pageSize;
        
        var postFilter = Builders<PostDoc>.Filter.And(Builders<PostDoc>.Filter.Eq(x => x.Id, request.PostId),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var post = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return Response<List<GetPostLikerResponse>>.Fail(PostErrorMessage.PostNotFound, 404);

        if (post.UserId != userId)
            return Response<List<GetPostLikerResponse>>.Fail(PostErrorMessage.NotPostOwner, 403);

        var likerIds = await mongoDocContext.Likes.Collection
            .Find(l => l.PostId == request.PostId && l.UserId == userId)
            .Skip(skip)  
            .Limit(pageSize)
            .Project(l => l.UserId)
            .ToListAsync(cancellationToken);

        if (likerIds.Count == 0)
            return Response<List<GetPostLikerResponse>>.Success(204);
        
        var users = await appDbContext.Users
            .Where(u => likerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var currentUserFollowings = await appDbContext.Follows
            .Where(f => f.FollowerId == userId && likerIds.Contains(f.FollowingId) && !f.IsDeleted)
            .Select(f => f.FollowingId)
            .ToListAsync(cancellationToken);

        var currentUserFollowers = await appDbContext.Follows
            .Where(f => f.FollowingId == userId && likerIds.Contains(f.FollowerId) && !f.IsDeleted)
            .Select(f => f.FollowerId)
            .ToListAsync(cancellationToken);

        var response = users.Select(u => new GetPostLikerResponse
        {
            UserId = u.Id,
            FullName = u.FullName,
            IsFollowing = currentUserFollowings.Contains(u.Id),
            IsFollowed = currentUserFollowers.Contains(u.Id)
        }).ToList();

        return Response<List<GetPostLikerResponse>>.Success(response);
    }
}
