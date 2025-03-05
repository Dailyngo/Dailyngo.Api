namespace EveryDaily.Domain.Prefix.Redis;

public class RedisPrefix
{
    public static string IsExistUserKey(string userId) => $"IsExistUser:{userId}";
    public static string TOKENS = "Tokens";
}