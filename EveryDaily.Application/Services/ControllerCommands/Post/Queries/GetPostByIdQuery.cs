using EveryDaily.Application.Dtos.Post.Responses;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Queries;

public class GetPostByIdQuery : IRequest<Response<GetUserPostResponse>>
{
    public string PostId { get; set; }
}

public class GetPostByIdQueryHandler(
    IUserService userService,
    MongoDocContext mongoDocContext,
    AppDbContext appDbContext)
    : IRequestHandler<GetPostByIdQuery, Response<GetUserPostResponse>>
{
    public async Task<Response<GetUserPostResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var objectId = ObjectId.Parse(request.PostId);
        var currentUserId = userService.GetUserId().ToString();

        var filter = Builders<PostDoc>.Filter.And(
            Builders<PostDoc>.Filter.Eq(x => x.Id, objectId),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false)
        );

        var post = await mongoDocContext.Posts.Collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (post == null)
            return Response<GetUserPostResponse>.Fail("Gönderi bulunamadı.", 404);
        
        var posterUserId = Guid.Parse(post.UserId);

        var poster = await appDbContext.Users
            .Select(x => new { x.Id, x.UserName })
            .FirstOrDefaultAsync(x => x.Id == posterUserId, cancellationToken);
        
        var likeFilter = Builders<LikeDoc>.Filter.And(
            Builders<LikeDoc>.Filter.Eq(x => x.PostId, post.Id),
            Builders<LikeDoc>.Filter.Eq(x => x.UserId, currentUserId),
            Builders<LikeDoc>.Filter.Eq(x => x.IsDeleted, false)
        );

        var isLiked = await mongoDocContext.Likes.Collection
            .Find(likeFilter)
            .AnyAsync(cancellationToken);

        var response = new GetUserPostResponse
        {
            Id = post.Id.ToString(),
            UserName = poster.UserName, 
            UserId = Guid.TryParse(post.UserId, out var parsedUserId) ? parsedUserId : Guid.Empty,
            Content = post.Content,
            IsOwner = post.UserId == currentUserId,
            IsLiked = isLiked,
            PostDate = post.CreatedAt,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            ImageKey = post.ImageUrl,
        };

        return Response<GetUserPostResponse>.Success(response);
    }
}
