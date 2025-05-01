using EveryDaily.Application.Dtos.Message.Response;
using EveryDaily.Application.Dtos.Notification;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Application.Socket;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Documents;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence.MongoContext;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EveryDaily.Application.Services.ControllerCommands.Message.Queries;

public class GetMessagesQuery : IRequest<Response<List<GetMessagesResponse>>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; }
}

public class GetMessagesQueryHandler(
    MongoDocContext mongoDocContext,
    IUserService userService,
    IHubContext<NotificationHub> hubContext)
    : IRequestHandler<GetMessagesQuery, Response<List<GetMessagesResponse>>>
{
    public async Task<Response<List<GetMessagesResponse>>> Handle(GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = 25;
        var ownUserId = userService.GetUserId();

        var filter1 = Builders<MessageDoc>.Filter.And(
            Builders<MessageDoc>.Filter.Eq(m => m.ReceiverId, request.UserId.ToString()),
            Builders<MessageDoc>.Filter.Eq(m => m.SenderId, ownUserId.ToString())
        );
        var filter2 = Builders<MessageDoc>.Filter.And(
            Builders<MessageDoc>.Filter.Eq(m => m.ReceiverId, ownUserId.ToString()),
            Builders<MessageDoc>.Filter.Eq(m => m.SenderId, request.UserId.ToString())
        );

        var filter = Builders<MessageDoc>.Filter.Or(filter1, filter2);

        var messages = await mongoDocContext.Messages.Collection
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var response = messages.Select(m => new GetMessagesResponse
            {
                Id = m.Id.ToString(),
                Content = m.Content,
                CreatedAt = m.CreatedAt.Value,
                IsOwner = m.SenderId == ownUserId.ToString()
            }).OrderByDescending(m => m.CreatedAt)
            .ToList();

        var updateFilter = Builders<MessageDoc>.Filter.And(
            Builders<MessageDoc>.Filter.Eq(m => m.ReceiverId, ownUserId.ToString()),
            Builders<MessageDoc>.Filter.Eq(m => m.SenderId, request.UserId.ToString()),
            Builders<MessageDoc>.Filter.Eq(m => m.IsRead, false)
        );

        var update = Builders<MessageDoc>.Update.Set(m => m.IsRead, true);

        await mongoDocContext.Messages.Collection.UpdateManyAsync(updateFilter, update,
            cancellationToken: cancellationToken);

        var totalMessageNotificationCount = await mongoDocContext.Messages.Collection
            .CountDocumentsAsync(m => m.ReceiverId == ownUserId.ToString() && !m.IsRead && !m.IsDeleted,
                cancellationToken: cancellationToken);

        await hubContext.Clients.Group(ownUserId.ToString()).SendAsync(
            NotificationHubMethods.ReceiveMessageNotification, totalMessageNotificationCount,
            cancellationToken: cancellationToken);

        return Response<List<GetMessagesResponse>>.Success(response);
    }
}