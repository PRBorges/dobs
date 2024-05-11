using Microsoft.Extensions.Logging;

namespace RateProvider;

/// <summary>
/// Provides extension methods for the ILogger interface
/// for loggging messages for the BcvRates class.
/// </summary>
/// <remarks>
/// These methods utilize the `LoggerMessage` attribute to define the log message template,
/// event ID, and log level.
/// </remarks>
public static partial class RateProviderLogger
{
    [LoggerMessage(EventId = 25, Level = LogLevel.Information, Message = "Getting new rate")]
    public static partial void GettingRate(this ILogger logger);

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Warning,
        Message = "Failed reading BCV page: {message}"
    )]
    public static partial void FailedReadingBCVPage(this ILogger logger, string message);

    [LoggerMessage(
        EventId = 21,
        Level = LogLevel.Warning,
        Message = "Could not extract rate from HTML: {message}"
    )]
    public static partial void CouldNotGetRateFromHtml(this ILogger logger, string message);
}
