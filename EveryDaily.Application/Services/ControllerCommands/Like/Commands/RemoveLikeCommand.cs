using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Like.Commands;

public class RemoveLikeCommand : IRequest<Response<NoContent>>
{
    public ObjectId PostId { get; set; }
}

public class RemoveLikeCommandHandler(MongoDocContext mongoDocContext, IUserService userService)
    : IRequestHandler<RemoveLikeCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(RemoveLikeCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var postFilter = Builders<PostDoc>.Filter.And(Builders<PostDoc>.Filter.Eq(x => x.Id, request.PostId),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var post = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);
        
        var filter = Builders<LikeDoc>.Filter.And(
            Builders<LikeDoc>.Filter.Eq(p => p.UserId, userId.ToString()),
            Builders<LikeDoc>.Filter.Eq(p => p.PostId, request.PostId)
        );

        var like = mongoDocContext.Likes.Collection
            .Find(filter)
            .FirstOrDefault(cancellationToken: cancellationToken);

        if (like == null)
            return Response<NoContent>.Success(204);

        await mongoDocContext.Likes.Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        var updatePost = Builders<PostDoc>.Update
            .Inc(p => p.LikeCount, -1);

        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost,
            cancellationToken: cancellationToken);

        return Response<NoContent>.Success(200);
    }
}