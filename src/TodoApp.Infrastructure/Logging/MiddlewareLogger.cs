using Microsoft.Extensions.Logging;

namespace TodoApp.Infrastructure.Logging;

public class MiddlewareLogger: ILogger
{
    public List<string> Logs { get; } = new();

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var log = formatter(state, exception);
        Logs.Add($"[{logLevel}] {log}");
    }
}