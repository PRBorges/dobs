using Microsoft.Extensions.Logging;

namespace Dobs.Data;

/// <summary>
/// Provides extension methods for the ILogger interface
/// for loggging messages for the DataManager class.
/// </summary>
/// <remarks>
/// These methods utilize the `LoggerMessage` attribute to define the log message template,
/// event ID, and log level.
/// </remarks>

public static partial class AppDataLogger
{
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Could not read AppData (this is normal if this is the first use): {message}"
    )]
    public static partial void CouldNotReadAppData(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Could not write AppData file: {message}"
    )]
    public static partial void CouldNotWriteAppData(this ILogger logger, string message);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Updating rate.")]
    public static partial void UpdatingData(this ILogger logger);

    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Information,
        Message = "No new rate available yet."
    )]
    public static partial void RateIsNotNew(this ILogger logger);
}
