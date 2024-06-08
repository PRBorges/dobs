using Dobs.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RateProvider;
using RateProvider.Types;

namespace Dobs.Tests.BcvRatesTests;

/// <summary>
/// Unit tests for the BcvRates class.
/// </summary>
/// <param name="clientFixture">An instance of HttpClientFixture to be used for HTTP requests.</param>
public class BcvRatesTest(HttpClientFixture clientFixture) : IClassFixture<HttpClientFixture>
{
    private readonly HttpClientFixture _clientFixture = clientFixture;

    [SkippableFact]
    public async Task GetBcvRateAgreesWithExchangeDynRate()
    {
        var rDynRate = await ExchangeDyn
            .GetRateAsync(this._clientFixture.Client)
            .ConfigureAwait(false);
        if (rDynRate.IsFailure)
        {
            throw new SkipException($"Could not get ExchangeDyn rate: {rDynRate.Error}");
        }
        var dynRate = rDynRate.Value;

        var bcvRates = new BcvRates(NullLogger.Instance);

        var mRate = await bcvRates.GetUSDRateAsync(TestData.BcvUri).ConfigureAwait(false);
        Skip.If(mRate.HasNoValue, "Could not get USD rate from BCV");
        var bcvRate = mRate.Value;

        Skip.If(DynRateIsBehind(dynRate, bcvRate), "ExchangeDyn rate has nott been updated yet.");

        bcvRate.Multiplier.Should().Be(dynRate.usdRate);
    }

    [Fact]
    public async Task GetBcvRateFailsWithOtherUri()
    {
        var bcvRates = new BcvRates(NullLogger.Instance);
        var otherUri = new Uri("https://www.example.com");

        var mRate = await bcvRates.GetUSDRateAsync(otherUri).ConfigureAwait(false);

        mRate.Should().HaveNoValue();
    }

    /// <summary>
    /// Private helper method to check if the  rate from ExchangeDyn
    /// has not been update to be in sync with the BCV rate.
    /// </summary>
    /// <param name="dynRate">A triplet with the details of the ExchangeDyn rate.</param>
    /// <param name="bcvRate">The rate obtained from BCV.</param>
    /// <returns>True if ExchangeDyn rate is not up to date with BcvRate.
    private static bool DynRateIsBehind(
        (decimal usdRate, DateOnly date, TimeOnly time) dynRate,
        Rate<Usd, Ves> bcvRate
    ) =>
        bcvRate.Date > dynRate.date
        && IsDayOfWeek(dynRate.date)
        && dynRate.time < TestData.RateChangeTimeUtc;

    private static bool IsDayOfWeek(DateOnly date) =>
        date.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
}

/// <summary>
/// Provides an HttpClient for the BcvRates tests, using xunit's fixtures.
/// </summary>
public sealed class HttpClientFixture : IDisposable
{
    public HttpClientFixture() => Client = new HttpClient();

    public void Dispose() => Client.Dispose();

    public HttpClient Client { get; private set; }
}
