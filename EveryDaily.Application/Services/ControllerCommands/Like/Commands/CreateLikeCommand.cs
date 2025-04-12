using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Like.Commands;

public class CreateLikeCommand : IRequest<Response<NoContent>>
{
    public ObjectId PostId { get; set; }
}

public class CreateLikeCommandHandler(MongoDocContext mongoDocContext, IUserService userService)
    : IRequestHandler<CreateLikeCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(CreateLikeCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var postFilter = Builders<PostDoc>.Filter.And(Builders<PostDoc>.Filter.Eq(x => x.Id, request.PostId),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var post = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);
        
        var like = await mongoDocContext.Likes.Collection
            .Find(p => p.UserId == userId.ToString() && p.PostId == request.PostId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (like != null)
            return Response<NoContent>.Success(204);

        var newLike = new LikeDoc
        {
            PostId = request.PostId,
            UserId = userId.ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await mongoDocContext.Likes.Collection.InsertOneAsync(newLike, cancellationToken: cancellationToken);

        var updatePost = Builders<PostDoc>.Update
            .Inc(p => p.LikeCount, 1);

        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost,
            cancellationToken: cancellationToken);

        return Response<NoContent>.Success(204);
    }
}