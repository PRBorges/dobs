using System.Diagnostics.CodeAnalysis;
using CliFx.Infrastructure;
using RateProvider.Types;

namespace Dobs;

/// <summary>
/// Responsible for writing the result of the conversion command to the output console.
/// </summary>
public static class Display
{
    private const string Separator = "|";

    /// <summary>
    /// Writes the conversion results to the given console.
    /// </summary>
    /// <param name="output">An instance of ConsoleWriter used for writing to the console.</param>
    /// <param name="results">An enumerable collection of tuples containing the 
    /// converted currency and the date of the rate used for the conversion.</param>
    public static void DisplayConversions([NotNull] ConsoleWriter output, [NotNull] IEnumerable<(Currency, DateOnly)> results, int decimalsToDisplay)
    {
        foreach (var (currency, date) in results)
        {
            output.WriteLine(
                $"{currency.WithDecimals(decimalsToDisplay)} {Separator} {date}");
        }
    }
}
