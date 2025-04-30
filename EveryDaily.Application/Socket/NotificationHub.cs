using System.Collections.Concurrent;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using EveryDaily.Persistence.MongoContext;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EveryDaily.Application.Socket
{
    public class NotificationHub : Hub
    {
        private readonly MongoDocContext _mongoDocContext;
        private readonly IRedisService _redisService;
        private readonly IUserService _userService;
        public static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new();

        public NotificationHub(IRedisService redisService, IUserService userService, MongoDocContext mongoDocContext)
        {
            _redisService = redisService;
            _userService = userService;
            _mongoDocContext = mongoDocContext;
        }

        /// <summary>
        /// Kullanıcı bağlantı açtığında offline bildirimleri gönder ve gruba ekle
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = _userService.GetUserId().ToString();
            var connectionId = Context.ConnectionId;

            // Kullanıcıyı kendi ID'siyle gruba ekle
            await Groups.AddToGroupAsync(connectionId, userId);

            // OnlineUsers'a ekle (çoklu bağlantı destekli)
            OnlineUsers.AddOrUpdate(userId,
                (_) => new HashSet<string> { connectionId },
                (_, set) =>
                {
                    lock (set)
                    {
                        set.Add(connectionId);
                    }
                    return set;
                });

            await base.OnConnectedAsync();
            await SendOfflineNotifications(userId);
        }
        /// <summary>
        /// Kullanıcı bağlantıyı kapattığında gruptan çıkar
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userService.GetUserId().ToString();

            // Kullanıcıyı grubundan çıkar
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            
            var connectionId = Context.ConnectionId;
            if (OnlineUsers.TryGetValue(userId, out var set))
            {
                lock (set)
                {
                    set.Remove(connectionId);
                    if (set.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Kullanıcı offline iken gelen bildirimlerin sayısını önce Redis'ten, eğer yoksa MongoDB'den çekip gönderir.
        /// </summary>
        private async Task SendOfflineNotifications(string userId)
        {
            long totalNotificationCount = 0;

            string redisKey = RedisPrefix.GetUserNotificationsKey(Guid.Parse(userId));

            // Redis'teki key var mı kontrol et
            bool redisKeyExists = await _redisService.ExistsAsync(redisKey);

            if (redisKeyExists)
            {
                // Redis'teki bildirim sayısını al
                totalNotificationCount = await _redisService.ListLengthAsync(redisKey);
            }
            else
            {
                totalNotificationCount = await _mongoDocContext.Notifications.Collection
                    .CountDocumentsAsync(n => n.ReceiverId == userId && !n.IsRead && !n.IsDeleted);
            }

            // Kullanıcıya bildirim sayısını gönder (User yerine Group kullanıldı)
            await Clients.Group(userId).SendAsync(NotificationHubMethods.ReceiveNotification, totalNotificationCount);
        }
    }
}
