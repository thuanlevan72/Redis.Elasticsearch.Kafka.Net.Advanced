using StackExchange.Redis;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Infrastructure.Redis;

public class PubSubService: IPubSubService
{
    private readonly IConnectionMultiplexer _redis;

    public PubSubService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync(string channel, string message)
    {
        var sub = _redis.GetSubscriber();
        await sub.PublishAsync(channel, message);
    }

    public async Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler)
    {
        var sub = _redis.GetSubscriber();
        await sub.SubscribeAsync(channel, handler);
    }
}