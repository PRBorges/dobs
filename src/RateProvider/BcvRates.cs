using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using RateProvider.BcvScraper;
using RateProvider.Types;

namespace RateProvider;

/// <summary>
/// Responsible for retrieving USD to VES exchange rates from BCV website.
/// </summary>
public class BcvRates(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Asynchronously retrieves the USD to VES exchange rate (Rate<Usd, Ves>) 
    /// from the provided BCV Uri.
    /// </summary>
    /// <param name="bcvUri">Uri of the BCV webpage containing the exchange rate.</param>
    /// <returns>
    /// A Maybe containing the retrieved exchange rate, or Maybe.None if an error occurred.
    /// </returns>
    public async Task<Maybe<Rate<Usd, Ves>>> GetUSDRateAsync(Uri bcvUri)
    {
        _logger.GettingRate();
        var rRateTask = HtmlLoader
            .GetUriContentAsync(bcvUri)
            .TapError(this._logger.FailedReadingBCVPage)
            .Bind(Scraper.ExtractRateAsync)
            .TapError(_logger.CouldNotGetRateFromHtml);
        var rRate = await rRateTask.ConfigureAwait(false);
        return rRate.IsSuccess ? Maybe.From(rRate.Value) : Maybe.None;
    }
}
