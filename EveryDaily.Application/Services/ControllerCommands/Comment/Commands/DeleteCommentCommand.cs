using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Comment.Commands;

public class DeleteCommentCommand : IRequest<Response<NoContent>>
{
    public ObjectId Id { get; set; }
}

public class DeleteCommentCommandHandler(MongoDocContext mongoDocContext, IUserService userService)
    : IRequestHandler<DeleteCommentCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var comment = await mongoDocContext.Comments.Collection
            .Find(p => p.Id == request.Id && p.UserId == userId.ToString())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (comment == null)
            return Response<NoContent>.Fail(CommentErrorMessage.CommentNotFound, 404);
        
        var update = Builders<CommentDoc>.Update
            .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow)
            .Set(p => p.IsDeleted, true);

        var filter = Builders<CommentDoc>.Filter.And(
            Builders<CommentDoc>.Filter.Eq(p => p.Id, request.Id),
            Builders<CommentDoc>.Filter.Eq(p => p.UserId, userId.ToString())
        );

        await mongoDocContext.Comments.Collection.UpdateOneAsync(filter, update,
            cancellationToken: cancellationToken);

        var replyFilter = Builders<CommentDoc>.Filter.And(
            Builders<CommentDoc>.Filter.Eq(p => p.ReplyCommentId, request.Id),
            Builders<CommentDoc>.Filter.Eq(p => p.IsDeleted, false));

        var replyUpdate = Builders<CommentDoc>.Update.Set(p => p.IsDeleted, true)
            .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow);

        await mongoDocContext.Comments.Collection.UpdateManyAsync(replyFilter, replyUpdate,
            cancellationToken: cancellationToken);
        
        var updatePost = Builders<PostDoc>.Update
            .Inc(p => p.CommentCount, -1);

        var postFilter = Builders<PostDoc>.Filter.Eq(p => p.Id, comment.PostId);
        
        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost,
            cancellationToken: cancellationToken);

        return Response<NoContent>.Success(200);
    }
}