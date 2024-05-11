namespace RateProvider.Types;

/// <summary>
/// Represents a base abstract record for different currencies.
/// </summary>
public abstract record Currency
{
    protected abstract string Prefix { get; }

    public decimal Amount { get; init; }

    /// <summary>
    /// Initializes a new instance of the Currency class with the default amount (0).
    /// </summary>
    protected Currency() => Amount = default;

    /// <summary>
    /// Creates a new instance of the same currency with the amount 
    /// rounded to the specified number of decimal places.
    /// </summary>
    /// <param name="numberOfDecimals">The number of decimal places to round the Amount to.</param>
    /// <returns>A new Currency object with the rounded Amount.</returns>
    public Currency WithDecimals(int numberOfDecimals)
        => this with { Amount = decimal.Round(this.Amount, numberOfDecimals) };

    /// <summary>
    /// Provides a string representation of the currency in the format "{Prefix} {Amount}".
    /// </summary>
    /// <returns>A string representation of the c
    /// urrency.</returns>
    public override string ToString() => $"{this.Prefix} {this.Amount}";

    /// <summary>
    /// Converts a currency to another based on the provided exchange rate.
    /// </summary>
    /// <typeparam name="TFrom">The type of the source currency (must be the 
    /// source type of the "rate" parameter).</typeparam>
    /// <typeparam name="TTo">The type of the target currency.</typeparam>
    /// <param name="rate">The exchange rate between the source and target currencies.</param>
    /// <returns>A new instance of the target currency with the converted amount.</returns>
    public TTo Convert<TFrom, TTo>(Rate<TFrom, TTo> rate)
        where TFrom : Currency
        where TTo : Currency, new()
    {
        ArgumentNullException.ThrowIfNull(rate);
        return new TTo() with { Amount = this.Amount * rate.Multiplier };
    }

    /// <summary>
    /// Converts a currency to another based on the provided exchange rate.
    /// </summary>
    /// <typeparam name="TFrom">The type of the source currency (must be the 
    /// target type of the "rate" parameter).</typeparam>
    /// <typeparam name="TTo">The type of the target currency.</typeparam>
    /// <param name="rate">The exchange rate between the target and source currencies.</param>
    /// <returns>A new instance of the target currency with the converted amount.</returns>
    /// <exception cref="DivideByZeroException">Thrown if the exchange rate multiplier is zero.</exception>
    public TTo Convert<TFrom, TTo>(Rate<TTo, TFrom> rate)
        where TFrom : Currency
        where TTo : Currency, new()
    {
        ArgumentNullException.ThrowIfNull(rate);
        return new TTo() with { Amount = this.Amount / rate.Multiplier };
    }


}

/// <summary>
/// Represents the Venezuelan Bolivar currency.
/// </summary>
public record Ves : Currency
{
    protected override string Prefix => "Bs.";

    /// <summary>
    /// Initializes a new instance of the Ves record with the default amount (0).
    /// </summary>
    public Ves()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the Ves record with the specified amount.
    /// </summary>
    /// <param name="amount">The amount of the currency.</param>
    public Ves(decimal amount) => Amount = amount;

    /// <summary>
    /// Provides a string representation of the Ves currency using the base class implementation.
    /// </summary>
    /// <returns>A string representation of the Ves currency.</returns>
    public override string ToString() => base.ToString();
}

/// <summary>
/// Represents the US Dollar currency.
/// </summary>
public record Usd : Currency
{
    protected override string Prefix => "US$";

    /// <summary>
    /// Initializes a new instance of the Usd record with the default amount (0).
    /// </summary>
    public Usd()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the Usd record with the specified amount.
    /// </summary>
    /// <param name="amount">The amount of the currency.</param>
    public Usd(decimal amount) => Amount = amount;

    /// <summary>
    /// Provides a string representation of the Usd currency using the base class implementation.
    /// </summary>
    /// <returns>A string representation of the Usd currency.</returns>
    public override string ToString() => base.ToString();
}
