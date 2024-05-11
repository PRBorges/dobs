using CSharpFunctionalExtensions;

namespace RateProvider.BcvScraper;

/// <summary>
/// Provides methods for loading HTML content from a given Uri.
/// </summary>
internal static class HtmlLoader
{
    /// <summary>
    /// Asynchronously fetches the content of the given Uri.
    /// </summary>
    /// <param name="uri">The Uri of the resource to download.</param>
    /// <returns>
    /// A Result object containing either the downloaded string content or 
    /// an error message describing the failure.
    /// </returns>
    public static async Task<Result<string>> GetUriContentAsync(Uri uri)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var responseBody = await client.GetStringAsync(uri).ConfigureAwait(false);
                return Result.Success(responseBody);
            }
        }
        catch (Exception ex)
            when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            return Result.Failure<string>($"Problem loading {uri}: {ex.Message}");
        }
    }
}
