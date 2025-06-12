namespace TodoApp.Application.Common.Interfaces;

public interface IRedisCacheService: ICacheService
{
    Task<IEnumerable<string>> GetAllKeysAsync(string pattern = "*");
}