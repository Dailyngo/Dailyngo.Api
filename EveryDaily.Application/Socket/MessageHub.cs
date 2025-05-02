using System.Collections.Concurrent;
using EveryDaily.Application.Dtos.Message.Response;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Documents;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence.MongoContext;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EveryDaily.Application.Socket;

public class MessageHub : Hub
{
    private readonly MongoDocContext _mongoDocContext;
    private readonly IUserService _userService;
    private readonly IHubContext<NotificationHub> hubContext;
    private static readonly ConcurrentDictionary<string, HashSet<string>> ConnectedUsers = new();

    public MessageHub(IUserService userService, MongoDocContext mongoDocContext , IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
        _userService = userService;
        _mongoDocContext = mongoDocContext;
    }

    public override Task OnConnectedAsync()
    {
        var userId = _userService.GetUserId().ToString();

        var connectionId = Context.ConnectionId;
        ConnectedUsers.AddOrUpdate(userId,
            (_) => new HashSet<string> { connectionId },
            (_, set) =>
            {
                lock (set)
                {
                    set.Add(connectionId);
                }
                return set;
            });
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userService.GetUserId().ToString();
        var connectionId = Context.ConnectionId;

        if (!ConnectedUsers.TryGetValue(userId, out var set)) return base.OnDisconnectedAsync(exception);
        
        lock (set)
        {
            set.Remove(connectionId);
            if (set.Count == 0)
            {
                ConnectedUsers.TryRemove(userId, out _);
            }
        }

        return base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(string userId, string message)
    {
        var senderId = _userService.GetUserId().ToString();
        var connectionIds = ConnectedUsers.GetValueOrDefault(userId) ?? new HashSet<string>();
        foreach (var connectionId in connectionIds)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", new MessageHubDto
            {
                SenderId = Guid.Parse(senderId),
                Message = message
            });
        }
        
        var messageDoc = new MessageDoc
        {
            SenderId = senderId,
            ReceiverId = userId,
            Content = message,
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = connectionIds.Any()
        };
        
        await _mongoDocContext.Messages.Collection.InsertOneAsync(messageDoc);
        
        if(!messageDoc.IsRead)
        {
            var totalMessageNotificationCount = await _mongoDocContext.Messages.Collection
                .CountDocumentsAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted);
            
            await hubContext.Clients.Group(userId).SendAsync(NotificationHubMethods.ReceiveMessageNotification, totalMessageNotificationCount);
        }
    }
}