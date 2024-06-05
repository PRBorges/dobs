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

        var dolarDiv = document.QuerySelector("#dolar");
        if (dolarDiv is null)
        {
            return Result.Failure<Rate<Usd, Ves>>("No dolar div");
        }

        var rMultiplier = dolarDiv
            .QuerySelector("strong")
            .ToResult("No strong element")
            .MapTry(
                strongElement =>
                    decimal.Parse(strongElement.TextContent.Trim(), new CultureInfo("es-VE")),
                _ => "Problem parsing usd multiplier"
            );
        if (rMultiplier.IsFailure)
        {
            return Result.Failure<Rate<Usd, Ves>>(rMultiplier.Error);
        }

        var rDate = dolarDiv
            .NextElementSibling.ToResult("No sibling for dolarDiv")
            .Bind(div =>
                div.QuerySelector("span[datatype='xsd:dateTime']").ToResult("No date found")
            )
            .MapTry(
                span =>
                    DateTime.Parse(span.GetAttribute("content")!, CultureInfo.InvariantCulture),
                _ => "No date or date in bad format"
            )
            .Map(DateOnly.FromDateTime);

        return rDate.IsSuccess
            ? Result.Success(new Rate<Usd, Ves>(rMultiplier.Value, rDate.Value))
            : Result.Failure<Rate<Usd, Ves>>(rDate.Error);
    }
}
