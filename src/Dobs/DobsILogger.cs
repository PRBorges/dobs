using Microsoft.Extensions.Logging;

namespace Dobs;

/// <summary>
/// Implementation of the ILogger interface that writes logs to a provided StreamWriter.
/// </summary>
internal sealed class DobsILogger(string categoryName, StreamWriter logWriter) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly StreamWriter _logWriter = logWriter;

    private LogLevel _logLevel = LogLevel.Information;

    public DobsILogger SetLogLevel(LogLevel level)
    {
        this._logLevel = level;
        return this;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= this._logLevel;

    /// <summary>
    /// Logs a message at the specified log level with the provided formatter and optional exception.
    /// </summary>
    /// <param name="logLevel">The log level of the message.</param>
    /// <param name="eventId">The event ID associated with the message. Ignored.</param>
    /// <param name="state">The state object to be formatted into the message.</param>
    /// <param name="exception">An optional exception associated with the message.</param>
    /// <param name="formatter">The formatter function used to create the message string.</param>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var levelName = logLevel == LogLevel.Information ? "Info" : logLevel.ToString();

        _logWriter.WriteLine($"{_categoryName} {levelName}: {message}");
    }
}
