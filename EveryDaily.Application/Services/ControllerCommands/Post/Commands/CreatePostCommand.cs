using EveryDaily.Application.Dtos.Post.Requests;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Commands;

public class CreatePostCommand : IRequest<Response<NoContent>>
{
    public CreatePostRequest Data { get; init; }
}

public class CreatePostCommandHandler(
    AppDbContext appDbContext,
    MongoDocContext mongoDocContext,
    IUserService userService)
    : IRequestHandler<CreatePostCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        if (request.Data.Id.HasValue)
        {
            // check exist
            var postExist = await mongoDocContext.Posts.Collection
                .Find(x => x.Id == request.Data.Id.Value && x.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
            if (postExist == null)
                return Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);
         
            var update = Builders<PostDoc>.Update
                .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow)
                .Set(p => p.Content, request.Data.Content);

            var filter = Builders<PostDoc>.Filter.Eq(p => p.Id, request.Data.Id.Value);
            
            await mongoDocContext.Posts.Collection.UpdateOneAsync(filter, update,
                cancellationToken: cancellationToken);
            
            return Response<NoContent>.Success(200);
        }

        var now = DateTimeOffset.UtcNow;
        var queryBuilder = await mongoDocContext.Posts.Collection
            .CountDocumentsAsync(x => x.UserId == userId && x.CreatedAt.Value.Date == now.Date,
                cancellationToken: cancellationToken);

        if (queryBuilder >= 10)
            return Response<NoContent>.Fail(PostErrorMessage.PostLimitExceeded, 400);

        var post = new PostDoc
        {
            Content = request.Data.Content,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        await mongoDocContext.Posts.Collection.InsertOneAsync(post, cancellationToken: cancellationToken);

        return Response<NoContent>.Success(201);
    }
}