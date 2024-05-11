using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CSharpFunctionalExtensions;
using RateProvider.Types;
using RateProvider;
using Dobs.Data;
using Dobs.Data.Types;

namespace Dobs;

/// <summary>
/// Provides the CliFx.ICommand of the dobs app.
/// </summary>
/// <param name="logger"> The ILogger logger to use.</param>
/// <param name="dataManager"> The IDataManager that provides the app data.</param>
[Command(Description = "Converts USD to BES (Bs) or viceversa.")]
public class ConvertCommand(ILogger logger, IDataManager dataManager) : ICommand
{
    private readonly ILogger _logger = logger;
    private readonly IDataManager _dataManager = dataManager;

    // Command parameter and options.

    [CommandParameter(
        0,
        IsRequired = false,
        Converter = typeof(DecimalBindingConverter),
        Description = "Amount to be converted to VES, or to USD if -u is given."
    )]
    public decimal Amount { get; init; } = 1m;

    /// <summary>
    /// Set from "-u" flag. Used to set <see cref="Conversion" /> field.
    /// </summary>
    [CommandOption("convert-to-us-dollars", 'u', Description = "Convert Amount to USD.")]
    public bool ConvertToUsDollars { get; init; } = false;

    [CommandOption(
        "last-rate-only",
        'l',
        Description = "Make conversion using only the last rate available."
    )]
    public bool LastRateOnly { get; init; } = false;

    [CommandOption(
        "decimals-to-display",
        'd',
        Description = "Number of decimals in the result to display.",
        Validators = new[] { typeof(NonNegativeValidator) }
        )]
    public int DecimalsToDisplay { get; init; } = 2;

    /// <summary>
    /// Determines the conversion type based on the command options.
    /// </summary>
    private ConversionType Conversion
        => this.ConvertToUsDollars ? ConversionType.VesToUsd : ConversionType.UsdToVes;

    /// <summary>
    /// Precision (number of decimals) for the rates to use in the conversions.
    /// </summary>
    private const int _precision = 2;

    /// <summary>
    /// Executes the conversion command and displays the results. Called by the CliFx app.
    /// </summary>
    /// <param name="console">The CliFx.IConsole to write the results to.</param>
    public async ValueTask ExecuteAsync([NotNull] IConsole console)
    {
        var data = await GetDataForConversionsAsync(_logger, _dataManager).ConfigureAwait(false);
        var ratesForConversion = getRatesForConversion(data, this.LastRateOnly, _precision);
        if (ratesForConversion.Count == 0)
        {
            throw new CommandException("No rates available", 1);
        }

        List<(Currency, DateOnly)> conversions;
        switch (this.Conversion)
        {
            case ConversionType.UsdToVes:
                var usdAmount = new Usd(Amount);
                conversions = ratesForConversion
                    .Select(r => (usdAmount.Convert<Usd, Ves>(r) as Currency, r.Date))
                    .ToList();
                break;
            case ConversionType.VesToUsd:
                var vesAmount = new Ves(Amount);
                try
                {
                    conversions = ratesForConversion
                        .Select(r => (vesAmount.Convert<Ves, Usd>(r) as Currency, r.Date))
                        .ToList();
                    break;
                }
                catch (DivideByZeroException)
                {
                    throw new CommandException("Can not make this conversion with rate 0.", 1);
                }
            default:
                throw new CommandException("Unknown conversion option.", 1);
        }

        Display.DisplayConversions(console.Output, conversions, this.DecimalsToDisplay);
    }

    /// <summary>
    /// Gets the app data with the rates to be used for conversions.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="dataManager">The data manager to access data.</param>
    /// <returns>The stored or default app data, updated with a 
    /// new rate if available/necessary.</returns>
    private static async Task<AppData> GetDataForConversionsAsync(ILogger logger, IDataManager dataManager)
    {
        var data = dataManager.GetAppData();
        if (data.IsUpdateTime())
        {
            var bcvRates = new BcvRates(logger);
            var mCurrentRate = await bcvRates.GetUSDRateAsync(data.SourceUri)
                .ConfigureAwait(false);
            var mNewData = dataManager.UpdateIfNewerRate(data, mCurrentRate);
            mNewData.Execute(newData =>
            {
                dataManager.StoreAppData(newData);
                data = newData;
            });
        }

        return data;
    }

    /// <summary>
    /// Gets the rates or rates to be used in the conversions according to the command options.
    /// </summary>
    /// <param name="data">The app data.</param>
    /// <param name="lastRateOnly">Whether to use only the last rate.</param>
    /// <param name="precision">The precision for the rates.</param>
    /// <returns>A list of rates for the conversion.</returns>
    private static List<Rate<Usd, Ves>> getRatesForConversion(AppData data, bool lastRateOnly, int _precision)
    {
        if (data.LastRate is null)
        {
            return new();
        }

        if (data.PreviousRate is null || lastRateOnly)
        {
            return new() { data.LastRate.WithPrecision(_precision) };
        }

        return new()
            { data.PreviousRate.WithPrecision(_precision),
            data.LastRate.WithPrecision(_precision) };
    }

}
