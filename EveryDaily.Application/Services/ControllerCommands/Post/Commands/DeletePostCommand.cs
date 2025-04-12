using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Post.Commands;

public class DeletePostCommand : IRequest<Response<NoContent>>
{
    public ObjectId Id { get; set; }
}

public class DeletePostCommandHandler(MongoDocContext mongoDocContext, IUserService userService)
    : IRequestHandler<DeletePostCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();

        var update = Builders<PostDoc>.Update
            .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow)
            .Set(p => p.IsDeleted, true);
        
        var filter = Builders<PostDoc>.Filter.And(
            Builders<PostDoc>.Filter.Eq(p => p.Id, request.Id),
            Builders<PostDoc>.Filter.Eq(p => p.UserId, userId)
        );

        var result =
            await mongoDocContext.Posts.Collection.UpdateOneAsync(filter, update,
                cancellationToken: cancellationToken);

        return result.ModifiedCount == 0
            ? Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404)
            : Response<NoContent>.Success(200);
    }
}