using FluentAssertions;
using RateProvider.Types;
using RateProvider.BcvScraper;
using Dobs.Tests.Utils.FileProvider;

namespace Dobs.Tests.RateProviderTests;

/// <summary>
/// Unit tests for the Scraper class.
/// </summary>
public class ScraperTest
{
    [SkippableFact]
    public async Task CanExtractRateFromLocalHtml()
    {
        var localHtmlFile = "bcvRatePage.html";
        var html = EmbeddedFileReader.ReadAsString(localHtmlFile);
        Skip.If(html is null, $"{localHtmlFile} was not found.");

        var expectedRate = new Rate<Usd, Ves>(35.42980000m, new DateOnly(2023, 11, 17));

        var rRate = await Scraper.ExtractRateAsync(html).ConfigureAwait(false);

        rRate.Should().SucceedWith(expectedRate);
    }

    [SkippableFact]
    public async Task GetsRateFromOtherHtmlFails()
    {
        var otherFile = "example.com.html";
        var html = EmbeddedFileReader.ReadAsString(otherFile);
        Skip.If(html is null, $"{otherFile} was not dound.");

        var rRate = await Scraper.ExtractRateAsync(html).ConfigureAwait(false);

        rRate.Should().Fail();
        rRate.Error.Should().StartWith("No dolar div");
    }
}
