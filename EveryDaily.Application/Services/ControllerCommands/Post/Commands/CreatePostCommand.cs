using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Post.Requests;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Enums.Rank;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MassTransit;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Commands;

public class CreatePostCommand : IRequest<Core.Dtos.Response<NoContent>>
{
    public CreatePostRequest Data { get; init; }
}

public class CreatePostCommandHandler(
    AppDbContext appDbContext,
    MongoDocContext mongoDocContext,
    IBusControl busControl,
    IUserService userService)
    : IRequestHandler<CreatePostCommand, Core.Dtos.Response<NoContent>>
{
    public async Task<Core.Dtos.Response<NoContent>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId().ToString();

        if (request.Data.Id != null)
        {
            var postExist = await mongoDocContext.Posts.Collection
                .Find(x => x.Id == ObjectId.Parse(request.Data.Id) && x.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
            if (postExist == null)
                return Core.Dtos.Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);
         
            var update = Builders<PostDoc>.Update
                .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow)
                .Set(p => p.Content, request.Data.Content);

            var filter = Builders<PostDoc>.Filter.Eq(p => p.Id,ObjectId.Parse(request.Data.Id));
            
            await mongoDocContext.Posts.Collection.UpdateOneAsync(filter, update,
                cancellationToken: cancellationToken);
            
            return Core.Dtos.Response<NoContent>.Success(200);
        }

        var startOfToday = DateTime.UtcNow.Date;
        var startOfTomorrow = startOfToday.AddDays(1);
        
        var filterPostCount = Builders<PostDoc>.Filter.Eq(p => p.UserId, userId) &
                              Builders<PostDoc>.Filter.Where(p => p.CreatedAt >= startOfToday && p.CreatedAt < startOfTomorrow);
        
        var queryBuilder = await mongoDocContext.Posts.Collection
            .Find(filterPostCount)
            .CountDocumentsAsync(cancellationToken);

        if (queryBuilder >= 10)
            return Core.Dtos.Response<NoContent>.Fail(PostErrorMessage.PostLimitExceeded, 400);

        var post = new PostDoc
        {
            Content = request.Data.Content,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        await mongoDocContext.Posts.Collection.InsertOneAsync(post, cancellationToken: cancellationToken);

        await busControl.Publish(new RankActivityMessage()
        {
            UserId = userService.GetUserId(),
            ActivityType = XpActivityType.post
        },cancellationToken);

        return Core.Dtos.Response<NoContent>.Success(201);
    }
}