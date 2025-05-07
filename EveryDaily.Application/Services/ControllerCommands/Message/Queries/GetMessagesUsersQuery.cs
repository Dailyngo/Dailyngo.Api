using EveryDaily.Application.Dtos.Message.Response;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Message.Queries;

public class GetMessagesUsersQuery : IRequest<Response<List<GetMessagesUsersResponse>>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetMessagesUsersQueryHandler(
    MongoDocContext mongoDocContext,
    IUserService userService,
    AppDbContext appDbContext)
    : IRequestHandler<GetMessagesUsersQuery, Response<List<GetMessagesUsersResponse>>>
{
    public async Task<Response<List<GetMessagesUsersResponse>>> Handle(GetMessagesUsersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userService.GetUserId();
        var pageSize = request.PageSize;

        var filter = Builders<MessageDoc>.Filter.Or(
            Builders<MessageDoc>.Filter.Eq(m => m.ReceiverId, userId.ToString()),
            Builders<MessageDoc>.Filter.Eq(m => m.SenderId, userId.ToString())
        );

        var users = await mongoDocContext.Messages.Collection
            .Find(filter)
            .Project(p => p.SenderId == userId.ToString() ? p.ReceiverId : p.SenderId)
            .SortByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = users.Distinct()
            .Skip((request.PageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var usersResponses = await appDbContext.Users
            .Where(x => userIds.Contains(x.Id.ToString()))
            .Select(x => new GetMessagesUsersResponse
            {
                UserId = x.Id.ToString(),
                FullName = x.FullName,
                UnreadCount = 0,
                LastMessage = null
            })
            .ToListAsync(cancellationToken);

        var filterUserLastMessage1 = Builders<MessageDoc>.Filter.And(
            Builders<MessageDoc>.Filter.Eq(m => m.ReceiverId, userId.ToString()),
            Builders<MessageDoc>.Filter.In(m => m.SenderId, userIds)
        );

        var filterUserLastMessage2 = Builders<MessageDoc>.Filter.And(
            Builders<MessageDoc>.Filter.Eq(m => m.SenderId, userId.ToString()),
            Builders<MessageDoc>.Filter.In(m => m.ReceiverId, userIds)
        );

        var filterUserLastMessage = Builders<MessageDoc>.Filter.Or(filterUserLastMessage1, filterUserLastMessage2);

        var lastMessages = await mongoDocContext.Messages.Collection
            .Find(filterUserLastMessage)
            .SortByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var userResponse in usersResponses)
        {
            var lastMessage = lastMessages
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault(m => (m.SenderId == userResponse.UserId && m.ReceiverId == userId.ToString())
                                     || (m.ReceiverId == userResponse.UserId && m.SenderId == userId.ToString()));

            var lastMessagesUnreadCount = lastMessages
                .Count(m => m.SenderId == userResponse.UserId && !m.IsRead);

            if (lastMessage != null)
            {
                userResponse.LastMessage = lastMessage.SenderId == userId.ToString() ? null : lastMessage.Content;
                userResponse.LastMessageDate = lastMessage.CreatedAt.Value;
                userResponse.LastMessageOwner = lastMessage.SenderId == userId.ToString();
                userResponse.LastMessageReadDate = lastMessage.ReadDate;

                if (!string.IsNullOrEmpty(userResponse.LastMessage))
                {
                    if(userResponse.LastMessage.Contains("img"))
                       userResponse.LastMessage = "Bir resim gönderdi";
                }
            }

            userResponse.UnreadCount = lastMessagesUnreadCount;
        }

        return Response<List<GetMessagesUsersResponse>>.Success(usersResponses.OrderByDescending(x => x.LastMessageDate)
            .ToList());
    }
}