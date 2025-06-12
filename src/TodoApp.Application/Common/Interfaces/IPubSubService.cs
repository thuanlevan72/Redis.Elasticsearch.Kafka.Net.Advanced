using StackExchange.Redis;

namespace TodoApp.Application.Common.Interfaces;

public interface IPubSubService
{
    Task PublishAsync(string channel, string message);
    Task SubscribeAsync(string channel, Action<RedisChannel, RedisValue> handler);
}