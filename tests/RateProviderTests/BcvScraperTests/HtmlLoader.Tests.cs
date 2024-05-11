using FluentAssertions;
using RateProvider.BcvScraper;
using Dobs.Tests.Utils.FileProvider;

namespace Dobs.Tests.RateProviderTests;

/// <summary>
/// Unit tests for the HtmlLoader class.
/// </summary>
public class HtmlLoaderTest
{
    [SkippableFact]
    public async Task ReadsExampleUri()
    {
        var exampleUri = new Uri("https://www.example.com");
        var exampleHtmlFile = "example.com.html";
        var expectedHtml = EmbeddedFileReader.ReadAsString(exampleHtmlFile);
        Skip.If(expectedHtml is null, "Example html file was not found.");

        var rHtml = await HtmlLoader.GetUriContentAsync(exampleUri).ConfigureAwait(false);

        rHtml.Should().SucceedWith(expectedHtml);
    }

    [Fact]
    public async Task GetNonExistentUriFails()
    {
        var nonExistentUri = new Uri("https://example.com/dobs-non-existent-test");

        var rHtml = await HtmlLoader.GetUriContentAsync(nonExistentUri).ConfigureAwait(false);

        rHtml.Should().Fail("Uri to be read does not exists.");
        rHtml.Error.Should().StartWith("Problem loading " + nonExistentUri);
    }
}
