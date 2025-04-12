using EveryDaily.Application.Dtos.Comment.Requests;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Comment.Commands;

public class CreateCommentCommand : IRequest<Response<NoContent>>
{
    public CreateCommentRequest Data { get; set; }
}

public class CreateCommentCommandHandler(MongoDocContext mongoDocContext, IUserService userService)
    : IRequestHandler<CreateCommentCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var postFilter = Builders<PostDoc>.Filter.And(Builders<PostDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(request.Data.PostId)),
            Builders<PostDoc>.Filter.Eq(x => x.IsDeleted, false));

        var post = await mongoDocContext.Posts.Collection
            .Find(postFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            return Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);

        if (request.Data.ReplyCommentId != null)
        {
            var replyCommentFilter = Builders<CommentDoc>.Filter.And(
                Builders<CommentDoc>.Filter.Eq(x => x.Id, ObjectId.Parse(request.Data.ReplyCommentId)),
                Builders<CommentDoc>.Filter.Eq(x => x.IsDeleted, false));

            var replyCommentExist = await mongoDocContext.Comments.Collection
                .Find(replyCommentFilter)
                .AnyAsync(cancellationToken);
            
            if (!replyCommentExist)
                return Response<NoContent>.Fail(CommentErrorMessage.ReplyCommentNotFound, 404);
        }

        var comment = new CommentDoc()
        {
            UserId = userId.ToString(),
            Content = request.Data.Content,
            ReplyCommentId = request.Data.ReplyCommentId != null ? ObjectId.Parse(request.Data.ReplyCommentId) : null,
            CreatedAt = DateTime.UtcNow
        };
        
        await mongoDocContext.Comments.Collection.InsertOneAsync(comment, cancellationToken: cancellationToken);

        var updatePost = Builders<PostDoc>.Update.Inc(x => x.CommentCount, 1);
        await mongoDocContext.Posts.Collection.UpdateOneAsync(postFilter, updatePost, cancellationToken: cancellationToken);
        
        return Response<NoContent>.Success(204);
    }
}