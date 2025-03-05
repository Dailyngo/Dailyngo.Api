using System.Text.Json;
using System.Text.Json.Serialization;
using EveryDaily.Core.Dtos;
using StackExchange.Redis;

namespace EveryDaily.Core;

public interface IRedisService
{
    Task<string?> GetStringAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task DeleteAsync(string key);
    bool Connect();
    Task PublishSubscriberAsync(string subject, SubMessage message);
    Task PublishSubscriberAsync(string subject, string message);
    Task SubscribeAsync(string subject, Action<RedisChannel, RedisValue> handler);
}

public class RedisService : IRedisService
{
    private readonly Lazy<ConnectionMultiplexer> _redis;
    private ConnectionMultiplexer Redis => _redis.Value;

    public RedisService(string host, int port, ConfigurationOptions? options = null)
    {
        var config = options ?? new ConfigurationOptions
        {
            EndPoints = { $"{host}:{port}" },
            SyncTimeout = 10000,
        };
        _redis = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config));
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var db = GetDb();
        var result = await db.StringGetAsync(key);
        return result.HasValue ? result.ToString() : null;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var db = GetDb();
        return await db.KeyExistsAsync(key);
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        var db = GetDb();
        await db.StringSetAsync(key, value, expiration ?? TimeSpan.FromMinutes(30));
    }

    public async Task DeleteAsync(string key)
    {
        var db = GetDb();
        await db.KeyDeleteAsync(key);
    }

    public bool Connect()
    {
        return Redis.IsConnected;
    }

    private IDatabase GetDb(int db = 0)
    {
        return Redis.GetDatabase(db);
    }
    
    public async Task PublishSubscriberAsync(string subject, SubMessage message)
    {
        var sub = Redis.GetSubscriber();
        await sub.PublishAsync(RedisChannel.Literal(subject), JsonSerializer.Serialize(message));
    }

    public async Task PublishSubscriberAsync(string subject, string message)
    {
        var sub = Redis.GetSubscriber();
        await sub.PublishAsync(RedisChannel.Literal(subject), message);
    }

    public async Task SubscribeAsync(string subject, Action<RedisChannel, RedisValue> handler)
    {
        var sub = Redis.GetSubscriber();
        await sub.SubscribeAsync(RedisChannel.Literal(subject), handler);
    }
}