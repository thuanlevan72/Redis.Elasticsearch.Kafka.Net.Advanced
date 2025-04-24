using Microsoft.Extensions.Logging;

namespace TodoApp.Infrastructure.Logging;

public class MiddlewareLoggerProvider: ILoggerProvider
{
    private readonly MiddlewareLogger _logger = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose() { }

    public MiddlewareLogger GetLogger() => _logger;
}