using System.Globalization;
using System.Text.Json;
using CSharpFunctionalExtensions;

namespace Dobs.Tests.Utils;

/// <summary>
/// Provides a task to get the USD to VES with the ExchangeDyn API.
/// </summary>
public static class ExchangeDyn
{
    private static readonly Uri _exchangeDynUri = new Uri(
        "https://api.exchangedyn.com/markets/quotes/usdves/bcv"
    );

    /// <summary>
    /// Retrieves the latest USD/VES rate from ExchangeDyn.
    /// </summary>
    /// <param name="client">The HttpClient instance to use for the HTTP requests.</param>
    /// <returns>
    /// A Result object containing a 3-tuple of (USD rate, date, time) if successful,
    /// or a Failure with an error message otherwise.
    /// </returns>
    public static async Task<Result<(decimal usdRate, DateOnly date, TimeOnly time)>> GetRateAsync(
        HttpClient client
    )
    {
        ArgumentNullException.ThrowIfNull(client);
        var rJson = await GetJsonContent(client).ConfigureAwait(false);
        if (rJson.IsFailure)
        {
            return Result.Failure<(decimal usdRate, DateOnly date, TimeOnly time)>(
                $"Problem loading ExchangeDyn rate: {rJson.Error}"
            );
        }

        var rBcvElement = Result.Try(
            () =>
                JsonDocument
                    .Parse(rJson.Value)
                    .RootElement.GetProperty("sources")
                    .GetProperty("BCV")
        );
        if (rBcvElement.IsFailure)
        {
            return Result.Failure<(decimal usdRate, DateOnly date, TimeOnly time)>(
                "No BCV element in ExchangeDyn response."
            );
        }

        var rRateDateTime = Result.Try(
            () => rBcvElement.Value.GetProperty("last_retrieved").GetDateTimeOffset().UtcDateTime
        );
        if (rRateDateTime.IsFailure)
        {
            return Result.Failure<(decimal usdRate, DateOnly date, TimeOnly time)>(
                "Bad or no datetime in ExchangeDyn response."
            );
        }

        var rUsdRate = Result
            .Try(() => rBcvElement.Value.GetProperty("quote").ToString())
            .MapTry(str => decimal.Parse(str, CultureInfo.InvariantCulture));

        if (rUsdRate.IsFailure)
        {
            return Result.Failure<(decimal usdRate, DateOnly date, TimeOnly time)>(
                "Bsd or no USD rate in ExchangeDyn response"
            );
        }

        rRateDateTime.Value.Deconstruct(out var date, out var time);
        return Result.Success((rUsdRate.Value, date, time));
    }

    private static async Task<Result<string>> GetJsonContent(HttpClient client)
    {
        try
        {
            var json = await client.GetStringAsync(_exchangeDynUri).ConfigureAwait(false);
            return Result.Success(json);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return Result.Failure<string>(ex.Message);
        }
    }
}
