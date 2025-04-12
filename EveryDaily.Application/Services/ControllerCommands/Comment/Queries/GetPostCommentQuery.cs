using EveryDaily.Application.Dtos.Comment.Responses;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Comment.Queries;

public class GetPostCommentQuery : IRequest<Response<List<GetPostCommentResponse>>>
{
    public ObjectId PostId { get; set; }
    public int PageNumber { get; set; } = 1;
}

public class GetPostCommentQueryHandler(MongoDocContext mongoDocContext,IUserService userService,AppDbContext appDbContext) 
    : IRequestHandler<GetPostCommentQuery, Response<List<GetPostCommentResponse>>>
{
    public async Task<Response<List<GetPostCommentResponse>>> Handle(GetPostCommentQuery request, CancellationToken cancellationToken)
    {
        int pageSize = 20;
        int skip = (request.PageNumber - 1) * pageSize;
        var userId = userService.GetUserId();
        
        var postExist = await mongoDocContext.Posts.Collection
            .Find(x => x.Id == request.PostId && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (postExist == null)
            return Response<List<GetPostCommentResponse>>.Fail(PostErrorMessage.PostNotFound,404);

        var filter = Builders<CommentDoc>.Filter.And(
            Builders<CommentDoc>.Filter.Eq(x => x.PostId, request.PostId),
            Builders<CommentDoc>.Filter.Eq(x => x.IsDeleted, false)
        );

        var comments = await mongoDocContext.Comments.Collection
            .Find(filter)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        
        var userIds = comments.Select(c => c.UserId).Distinct().ToList();

        var userList = await appDbContext.Users
            .Where(u => userIds.Contains(u.Id.ToString()))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync(cancellationToken);

        var response = comments.Select(comment =>
        {
            var userName = userList.FirstOrDefault(u => u.Id.ToString() == comment.UserId)?.UserName;

            return new GetPostCommentResponse
            {
                Id = comment.Id.ToString(),
                ReplyCommentId = comment.ReplyCommentId.ToString(),
                UserId = Guid.Parse( comment.UserId),
                CanDelete = comment.UserId == userId.ToString(),
                UserName = userName,
                Content = comment.Content,
                CommentDate = comment.CreatedAt
            };
        }).ToList();

        return Response<List<GetPostCommentResponse>>.Success(response);
    }
}