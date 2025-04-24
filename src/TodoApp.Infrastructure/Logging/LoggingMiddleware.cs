using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TodoApp.Infrastructure.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareLoggerProvider> _logger;
    
    public LoggingMiddleware(RequestDelegate next, MiddlewareLoggerProvider loggerProvider, ILogger<MiddlewareLoggerProvider> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine(_logger);
        await _next(context); // xử lý tiếp

        // // Ví dụ log toàn bộ sau khi request xử lý xong
        // foreach (var log in _loggerProvider.GetLogger().Logs)
        // {
        //     Console.WriteLine("đây là log của nó");
        //     Console.WriteLine("📝 Middleware Log: " + log);
        //     // hoặc ghi vào file, Elastic, etc.
        // }
    }
}