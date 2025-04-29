using EveryDaily.Application.Dtos.Report.Request;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Report.Commands;

public class ReportPostCommand : IRequest<Response<NoContent>>
{
    public ReportPostRequest Data { get; init; }
}

public class ReportPostCommandHandler(
    IUserService userService,
    AppDbContext appDbContext,
    MongoDocContext mongoDocContext)
    : IRequestHandler<ReportPostCommand, Response<NoContent>>
{
    public async Task<Response<NoContent>> Handle(ReportPostCommand request, CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();
        var postId = request.Data.PostId;
        var reportReason = request.Data.ReportReason;

        var filterReport = Builders<ReportDoc>.Filter.And(
            Builders<ReportDoc>.Filter.Eq(r => r.PostId, ObjectId.Parse(postId)),
            Builders<ReportDoc>.Filter.Eq(r => r.UserId, userId.ToString()));

        var existReport = await mongoDocContext.Reports.Collection
            .Find(filterReport)
            .AnyAsync(cancellationToken: cancellationToken);

        if (existReport)
            return Response<NoContent>.Fail(ReportErrorMessage.AlreadyReported, 400);

        var filter = Builders<PostDoc>.Filter.And(
            Builders<PostDoc>.Filter.Eq(p => p.Id, ObjectId.Parse(postId)),
            Builders<PostDoc>.Filter.Eq(p => p.IsDeleted, false),
            Builders<PostDoc>.Filter.Not(Builders<PostDoc>.Filter.Eq(p => p.UserId, userId.ToString())));
        
        var existPost = await mongoDocContext.Posts.Collection
            .Find(filter)
            .AnyAsync(cancellationToken: cancellationToken);
        
        if (!existPost)
            return Response<NoContent>.Fail(PostErrorMessage.PostNotFound, 404);
        
        var reportDoc = new ReportDoc
        {
            PostId = ObjectId.Parse(postId),
            UserId = userId.ToString(),
            ReportReason = reportReason,
            IsProcess = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await mongoDocContext.Reports.Collection.InsertOneAsync(reportDoc, new(), cancellationToken);
        
        return Response<NoContent>.Success(new NoContent(), 200);
    }
}