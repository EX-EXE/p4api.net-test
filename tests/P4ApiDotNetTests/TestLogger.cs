using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace P4ApiDotNetTests;

internal class TestLogger(ITestOutputHelper testOutputHelper) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var format = formatter(state, exception);
        testOutputHelper.WriteLine($"[{logLevel}][{eventId}]{format}{(exception != null ? $"\n{exception}" : "")}");
    }
}
