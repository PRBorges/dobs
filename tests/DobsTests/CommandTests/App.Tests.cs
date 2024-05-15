using System.Globalization;
using System.Text.RegularExpressions;
using CliFx.Infrastructure;
using Dobs.Data.Types;
using Dobs.Tests.Utils;
using Dobs.Tests.Utils.FileProvider;
using FluentAssertions;

namespace Dobs.Test;

public class AppTest
{
    private static readonly AppData _exampleData = TestData.ExampleData;
    private static readonly IFormatProvider _culture = CultureInfo.InvariantCulture;
    private const string _vesPrefix = "Bs.";
    private const string _usdPrefix = "US$";
    private const string _separator = "|";
    private const int _precision = 2;

    [SkippableFact]
    public async Task ExitsWithErrorIfInvalidNumberOfDecimals()
    {
        var dataJson = EmbeddedFileReader.ReadAsString("appData.example.json");
        Skip.If(dataJson is null, "Example AppData json file not found.");
        var appDataPath = TempFileProvider.WithContent(dataJson);
        var args = new[] { "10", "-d", "-3" };

        var (exitCode, _) = await RunAndOutputAsString(appDataPath, args).ConfigureAwait(false);

        exitCode
            .Should()
            .NotBe(0, "App should return error code due to negative number of decimals.");
    }

    [SkippableFact]
    public async Task AppConsecutiveCallsOk()
    {
        var dataJson = EmbeddedFileReader.ReadAsString("appData.example.json");
        Skip.If(dataJson is null, "Example AppData json file not found.");
        var appDataPath = TempFileProvider.WithContent(dataJson);
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var args = Array.Empty<string>();
        // First run
        var amountStr = decimal.Round(_exampleData.LastRate!.Multiplier, 2).ToString(_culture);
        var dateStr = _exampleData.LastRate.Date.ToString(_culture);
        var expectedOutput = FormatOutput(_vesPrefix, amountStr, dateStr);

        var (exitCode, output) = await RunAndOutputAsString(appDataPath, args)
            .ConfigureAwait(false);

        exitCode.Should().Be(0, "First run should exit correctly.");
        var lines = output.Split(Environment.NewLine);
        lines.Length.Should().Be(5, "First run output should log and display results.");
        lines[0].Should().Contain("Getting new rate");
        lines[1].Should().Contain("Updating rate", "first run should get new rate.");
        lines[2]
            .Should()
            .Be(
                StripNewLine(expectedOutput),
                "First result of first run should be LastRate in AppData file."
            );
        lines[3]
            .Should()
            .MatchRegex(
                _vesPrefix + @" \d+\.\d+ " + _separator + @" \d+/\d+/\d+",
                "First run should display VES result with new rate."
            );
        lines[4].Should().BeEmpty();

        // Second run
        var (newRateMultiplier, newRateDateStr) = getValuesFromOutput(lines[3]);
        var amountToConvert = 3.141596m;
        args = new[] { amountToConvert.ToString(_culture), "-l", "-d", "4" };
        amountStr = decimal.Round(amountToConvert * newRateMultiplier, 4).ToString(_culture);
        expectedOutput = FormatOutput(_vesPrefix, amountStr, newRateDateStr);

        (exitCode, output) = await RunAndOutputAsString(appDataPath, args).ConfigureAwait(false);

        exitCode.Should().Be(0, "Second run should exit correctly.");
        Skip.If(
            output.Contains("Getting new rate"),
            "Getting new rate in second run. Wait a few minutes and run again."
        );
        output.Should().Be(expectedOutput, "Second run should display BES result.");

        // Third run
        amountToConvert = 100.1m;
        args = new[] { amountToConvert.ToString(_culture), "-ul" };
        amountStr = decimal.Round(amountToConvert / newRateMultiplier, 2).ToString(_culture);
        expectedOutput = FormatOutput(_usdPrefix, amountStr, newRateDateStr);

        (exitCode, output) = await RunAndOutputAsString(appDataPath, args).ConfigureAwait(false);

        exitCode.Should().Be(0, "Third run should exit correctly.");
        output.Should().Be(expectedOutput, "Third run should display USD result.");
    }

    private static async Task<(int, string)> RunAndOutputAsString(string dataPath, string[] args)
    {
        using (var console = new FakeInMemoryConsole())
        {
            var app = Program.BuildApp(console, dataPath);
            var envVars = new Dictionary<string, string>();

            var exitCode = await app.RunAsync(args, envVars).ConfigureAwait(false);

            return (exitCode, console.ReadOutputString());
        }
    }

    private static (decimal, string) getValuesFromOutput(string line)
    {
        var pattern = @"\D(\d+\.\d+)\D+(.+)$";
        var match = Regex.Match(line, pattern);
        var numberStr = match.Groups[1].Value;
        var dayStr = match.Groups[2].Value;
        if (numberStr.Length == 0 || dayStr.Length == 0)
        {
            throw new ArgumentException($"Output line is not valid: {line}");
        }

        var number = decimal.Parse(numberStr, _culture);
        return (decimal.Round(number, _precision), dayStr);
    }

    private static string FormatOutput(string prefix, string amount, string date) =>
        $"{prefix} {amount} {_separator} {date}{Environment.NewLine}";

    private static string StripNewLine(string str) =>
        str.Replace(Environment.NewLine, string.Empty, StringComparison.Ordinal);
}
