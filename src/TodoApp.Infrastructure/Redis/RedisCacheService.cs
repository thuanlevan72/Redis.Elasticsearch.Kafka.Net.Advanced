using System.Text.Json;
using TodoApp.Application.Common.Interfaces;
using StackExchange.Redis;

namespace TodoApp.Infrastructure.Redis;

public class RedisCacheService: IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json =  JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<IEnumerable<string>> GetAllKeysAsync(string pattern = "*")
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        return server.Keys(pattern: pattern).Select(k => k.ToString());
    }
}