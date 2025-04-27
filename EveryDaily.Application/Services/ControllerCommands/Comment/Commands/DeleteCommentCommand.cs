using EveryDaily.Application.Services.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Enums.Notification;
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

public class DeleteCommentCommandHandler(MongoDocContext mongoDocContext, IUserService userService, INotificationService notificationService)
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

        var activeCommentCount = await mongoDocContext.Comments.Collection
            .CountDocumentsAsync(Builders<CommentDoc>.Filter.And(
                Builders<CommentDoc>.Filter.Eq(p => p.PostId, comment.PostId),
                Builders<CommentDoc>.Filter.Eq(p => p.IsDeleted, false)),
                cancellationToken: cancellationToken);

        var updatePost = Builders<PostDoc>.Update
            .Set(p => p.CommentCount, activeCommentCount);

        var postFilter = Builders<PostDoc>.Filter.Eq(p => p.Id, comment.PostId);

        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost,
            cancellationToken: cancellationToken);

        var postUserId = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .Project(p => p.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        await notificationService.RemoveCommentNotificationAsync(
            postUserId.ToString(),
            userId.ToString(),
            comment.Id.ToString(),
            cancellationToken);

        return Response<NoContent>.Success(200);
    }
}