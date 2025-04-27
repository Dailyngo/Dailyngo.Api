using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Comment.Requests;
using EveryDaily.Application.Services.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Enums.Rank;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Comment.Commands;

public class CreateCommentCommand : IRequest<Core.Dtos.Response<NoContent>>
{
    public CreateCommentRequest Data { get; set; }
}

public class CreateCommentCommandHandler(
    MongoDocContext mongoDocContext,
    IUserService userService,
    IBusControl busControl,INotificationService notificationService)
    : IRequestHandler<CreateCommentCommand, Core.Dtos.Response<NoContent>>
{
    public async Task<Core.Dtos.Response<NoContent>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var postFilter = Builders<PostDoc>.Filter.And(Builders<PostDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(request.Data.PostId)),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var post = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return Core.Dtos.Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);

        if (request.Data.ReplyCommentId != null)
        {
            var replyCommentFilter = Builders<CommentDoc>.Filter.And(
                Builders<CommentDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(request.Data.ReplyCommentId)),
                Builders<CommentDoc>.Filter.Eq(x => x.IsDeleted, false));

            var replyCommentExist = await mongoDocContext.Comments.Collection
                .Find(replyCommentFilter)
                .AnyAsync(cancellationToken);
            
            if (!replyCommentExist)
                return Core.Dtos.Response<NoContent>.Fail(CommentErrorMessage.ReplyCommentNotFound, 404);
        }

        var comment = new CommentDoc()
        {
            UserId = userId.ToString(),
            Content = request.Data.Content,
            PostId = post.Id,
            ReplyCommentId = request.Data.ReplyCommentId != null ? ObjectId.Parse(request.Data.ReplyCommentId) : null,
            CreatedAt = DateTime.UtcNow
        };
        
        await mongoDocContext.Comments.Collection.InsertOneAsync(comment, cancellationToken: cancellationToken);

        var activeCommentCount = await mongoDocContext.Comments.Collection
            .CountDocumentsAsync(Builders<CommentDoc>.Filter.And(
                Builders<CommentDoc>.Filter.Eq(p => p.PostId, comment.PostId),
                Builders<CommentDoc>.Filter.Eq(p => p.IsDeleted, false)),
                cancellationToken: cancellationToken);

        var updatePost = Builders<PostDoc>.Update.Set(x => x.CommentCount, activeCommentCount);

        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost, cancellationToken: cancellationToken);

        if (post.UserId != userId.ToString())
        {
            await busControl.Publish(new RankActivityMessage
            {
                UserId = Guid.Parse(post.UserId),
                ActivityType = XpActivityType.comment,
            }, cancellationToken);
        
            await notificationService.SendNotification(
                post.UserId,
                userId.ToString(),
                comment.Id.ToString()
                ,NotificationType.Comment,cancellationToken);
        }

        return Core.Dtos.Response<NoContent>.Success(204);
    }
}