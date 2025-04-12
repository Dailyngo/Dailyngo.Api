using EveryDaily.Domain.Enums;
using EveryDaily.Domain.Enums.Notification;
using EveryDaily.Domain.Enums.Rank;

namespace EveryDaily.Domain.Prefix.Redis;

public class RedisPrefix
{
    public static string IsExistUserKey(string userId) => $"IsExistUser:{userId}";
    public static string TOKENS = "Tokens";
    public static string GetAccessTokenKey(Guid userId) => $"{TOKENS}:{userId}:{JwtTokenType.AccessToken.ToString()}";
    public static string GetRefreshTokenKey(Guid userId) => $"{TOKENS}:{userId}:{JwtTokenType.RefreshToken.ToString()}";
    public static string GetEmailVerificationKey(Guid userId) => $"EmailVerificationCode:{userId}";

    #region Notification
    public static string GetUserNotificationsKey(Guid userId) => $"notifications:{userId}";
    public static string GetUnreadNotificationsKey(Guid userId) => $"unread_notifications:{userId}";
    public static string GetNotificationKey(Guid notificationId) => $"notification:{notificationId}";
    #endregion

    #region Rank
    public static string GetUserRankActivityKey(Guid userId,XpActivityType xpActivityType) => $"userrankactivity:{userId}:{xpActivityType}";
    #endregion

}