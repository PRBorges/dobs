using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using RateProvider.Types;

namespace RateProvider.BcvScraper;

/// <summary>
/// Provides a task for scraping USD to VES exchange rate from BCV website.
/// </summary>
internal static class Scraper
{
    /// <summary>
    /// Extracts the USD to VES exchange rate from the provided HTML content.
    /// </summary>
    /// <param name="html">The HTML content containing the exchange rate information.</param>
    /// <returns>
    /// A Result object containing either the extracted Rate or an error message.
    /// </returns>
    public static async Task<Result<Rate<Usd, Ves>>> ExtractRateAsync(string html)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html)).ConfigureAwait(false);

        var rDolarDiv = document.QuerySelector("#dolar").ToResult("No dolar div");
        if (rDolarDiv.IsFailure)
        {
            return Result.Failure<Rate<Usd, Ves>>(rDolarDiv.Error);
        }
        var rUSDRate = rDolarDiv.Value
            .QuerySelector("strong")
            .ToResult("No strong element")
            .MapTry(
                IElement => decimal.Parse(IElement.TextContent.Trim(), new CultureInfo("es-VE")),
                ex => "Problem parsing usd rate"
            );
        if (rUSDRate.IsFailure)
        {
            return Result.Failure<Rate<Usd, Ves>>(rUSDRate.Error);
        }

        var RDateDiv = rDolarDiv.Value.NextElementSibling.ToResult("No sibling for dolarDiv");
        if (RDateDiv.IsFailure)
        {
            return Result.Failure<Rate<Usd, Ves>>(RDateDiv.Error);
        }

        var RDate = RDateDiv.Value
            .QuerySelector("span[datatype='xsd:dateTime']")
            .ToResult("Nodate")
            .BindTry(
                spa =>
                    Result.Success<DateTime, string>(
                        DateTime.Parse(spa.GetAttribute("content")!, CultureInfo.InvariantCulture)
                    ),
                _ => "Date in bad format"
            )
            .Map(DateOnly.FromDateTime);

        return RDate.IsSuccess
            ? Result.Success(new Rate<Usd, Ves>(rUSDRate.Value, RDate.Value))
            : Result.Failure<Rate<Usd, Ves>>(RDate.Error);
    }
}
