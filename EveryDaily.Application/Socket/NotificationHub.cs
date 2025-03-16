using EveryDaily.Application.Repositories;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Domain.Prefix.Socket;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace EveryDaily.Application.Socket
{
    public class NotificationHub : Hub
    {
        NotificationRepository _notificationRepository;
        private readonly IRedisService _redisService;
        private readonly IUserService _userService;

        public NotificationHub(IRedisService redisService, IUserService userService, NotificationRepository notificationRepository)
        {
            _redisService = redisService;
            _userService = userService;
            _notificationRepository=notificationRepository;
        }

        /// <summary>
        /// Kullanıcı bağlantı açtığında offline bildirimleri gönder
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = _userService.GetUserId();
            await SendOfflineNotifications(userId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Kullanıcı offline iken gelen bildirimlerin sayısını önce Redis'ten, eğer yoksa MongoDB'den çekip gönderir.
        /// </summary>
        private async Task SendOfflineNotifications(Guid userId)
        {
            long totalNotificationCount = 0;

            // Redis'teki key var mı kontrol et
            bool redisKeyExists = await _redisService.ExistsAsync(RedisPrefix.GetUserNotificationsKey(userId));

            if (redisKeyExists)
            {
                // Redis'teki bildirim sayısını al
                totalNotificationCount = await _redisService.ListLengthAsync(RedisPrefix.GetUserNotificationsKey(userId));
            }
            else
            {
                // MongoDB'deki okunmamış bildirimlerin sayısını al
                totalNotificationCount = await _notificationRepository
                    .CountDocumentsAsync(n => n.ReceiverId == userId && !n.IsRead && !n.IsDeleted);
            }

            // Kullanıcıya bildirim sayısını gönder
            await Clients.User(userId.ToString()).SendAsync(NotificationHubMethods.ReceiveNotification, totalNotificationCount);
        }



    }
}
