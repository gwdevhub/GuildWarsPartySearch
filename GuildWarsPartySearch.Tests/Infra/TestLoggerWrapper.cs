using Microsoft.Extensions.Logging;

namespace GuildWarsPartySearch.Tests.Infra;

public sealed class TestLoggerWrapper<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] [{eventId}]\n{formatter(state, exception)}");
    }
}
