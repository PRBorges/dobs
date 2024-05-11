using System.Text.Json.Serialization;

namespace RateProvider.Types;

/// <summary>
/// Represents the exchange rate between two currencies on a specific date.
/// </summary>
/// <typeparam name="TFrom">The type of the source currency.</typeparam>
/// <typeparam name="TTo">The type of the target currency.</typeparam>
public record Rate<TFrom, TTo>
    where TFrom : Currency
    where TTo : Currency
{
    [JsonRequired]
    public decimal Multiplier { get; init; }

    [JsonRequired]
    public DateOnly Date { get; init; }

    /// <summary>
    /// Initializes a new instance of the Rate class.
    /// </summary>
    /// <param name="multiplier">The exchange rate multiplier.</param>
    /// <param name="date">The date of the exchange rate.</param>
    public Rate(decimal multiplier, DateOnly date) =>
        (this.Multiplier, this.Date) = (multiplier, date);

    /// <summary>
    /// Creates a new Rate object with the specified precision applied to the Multiplier property.
    /// </summary>
    /// <param name="precision">The number of decimal places to round the Multiplier to.</param>
    /// <returns>A new Rate object with the rounded Multiplier.</returns>
    public Rate<TFrom, TTo> WithPrecision(int precision) =>
        this with
        {
            Multiplier = decimal.Round(this.Multiplier, precision)
        };

    /// <summary>
    /// Checks if the current rate is newer than another rate.
    /// </summary>
    /// <param name="other">The other rate to compare with.</param>
    /// <returns>True if the current rate is newer, False otherwise.</returns>
    public bool IsNewerThan(Rate<TFrom, TTo> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return this.Date > other.Date;
    }
}
