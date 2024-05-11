using System.Text.Json.Serialization;
using RateProvider.Types;

namespace Dobs.Data.Types;

/// <summary>
/// Encapsulates the data needed for the app runs.
///</summary>
public record AppData
{
    /// <summary>
    /// The last exchange rate obtained, nullable to indicate missing data.
    /// </summary>
    [JsonRequired]
    public Rate<Usd, Ves>? LastRate { get; init; }

    /// <summary>
    /// The previous exchange rate obtained, nullable.
    /// </summary>
    public Rate<Usd, Ves>? PreviousRate { get; set; }

    /// <summary>
    /// The UTC time at which a new rate should be available at the BCV website.
    /// </summary>
    [JsonRequired]
    public TimeOnly RateChangeUtcTime { get; init; }

    /// <summary>
    /// The URI of the BCV page from where the USD/VES rate is sourced.
    /// </summary>
    [JsonRequired]
    public Uri SourceUri { get; init; }

    /// <summary>
    /// Initializes a new AppData instance with the provided values.
    /// </summary>
    public AppData(
        Rate<Usd, Ves>? lastRate,
        Rate<Usd, Ves>? previousRate,
        TimeOnly rateChangeUtcTime,
        Uri sourceUri
    ) =>
        (this.LastRate, this.PreviousRate, this.RateChangeUtcTime, this.SourceUri) = (
            lastRate,
            previousRate,
            rateChangeUtcTime,
            sourceUri
        );

    /// <summary>
    /// Checks if it's time to update the exchange rate data.
    /// </summary>
    /// <returns>True if it's time to update, false otherwise.</returns>
    public bool IsUpdateTime()
    {
        if (this.LastRate is null)
        {
            return true;
        }
        var UtcUpdateTime = this.LastRate.Date.ToDateTime(this.RateChangeUtcTime);
        return DateTime.UtcNow >= UtcUpdateTime;
    }

    /// <summary>
    /// Creates a new AppData instance with the provided new rate.
    /// The LastRate becomes the PreviousRate, and the newRate becomes the LastRate.
    /// </summary>
    /// <param name="newRate">The new exchange rate.</param>
    /// <returns>A new AppData instance with updated rates.</returns>
    public AppData UpdatedWith(Rate<Usd, Ves> newRate) =>
        this with
        {
            PreviousRate = this.LastRate,
            LastRate = newRate
        };
}
