using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using Microsoft.AspNetCore.SignalR;

namespace EveryDaily.Application.Socket
{
    public class NotificationHub : Hub
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly IRedisService _redisService;
        private readonly IUserService _userService;

        public NotificationHub(IRedisService redisService, IUserService userService, NotificationRepository notificationRepository)
        {
            _redisService = redisService;
            _userService = userService;
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Kullanıcı bağlantı açtığında offline bildirimleri gönder ve gruba ekle
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = _userService.GetUserId().ToString();

            // Kullanıcıyı kendi ID'siyle gruba ekle
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

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
                // MongoDB'deki okunmamış bildirimlerin sayısını al
                totalNotificationCount = await _notificationRepository
                    .CountDocumentsAsync(n => n.ReceiverId == userId && !n.IsRead && !n.IsDeleted);
            }

            // Kullanıcıya bildirim sayısını gönder (User yerine Group kullanıldı)
            await Clients.Group(userId).SendAsync(NotificationHubMethods.ReceiveNotification, totalNotificationCount);
        }
    }
}
