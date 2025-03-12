using EveryDaily.Domain.Enums;

namespace EveryDaily.Domain.Prefix.Redis;

public class RedisPrefix
{
    public static string IsExistUserKey(string userId) => $"IsExistUser:{userId}";
    public static string TOKENS = "Tokens";
    public static string GetAccessTokenKey(Guid userId) => $"{TOKENS}:{userId}:{JwtTokenType.AccessToken.ToString()}";
    public static string GetRefreshTokenKey(Guid userId) => $"{TOKENS}:{userId}:{JwtTokenType.RefreshToken.ToString()}";
}