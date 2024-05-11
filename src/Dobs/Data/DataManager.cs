using Microsoft.Extensions.Logging;
using CSharpFunctionalExtensions;
using Dobs.Data.Types;
using RateProvider.Types;
using System.Diagnostics.CodeAnalysis;

namespace Dobs.Data;

/// <summary>
/// Provides logging methods to manage app data.
/// </summary>
/// <param name="logger">The logger instance for logging operations.</param>
/// <param name="dataPath">The full path to the file where AppData is stored.</param>
public class DataManager(ILogger logger, string dataPath) : IDataManager
{
    private readonly ILogger _logger = logger;
    private readonly string _dataPath = dataPath;

    private static readonly AppData _defaultData =
    new AppData(
        null,
        null,
        BcvParams.RateChangeTimeUtc,
        BcvParams.BcvUri
    );

    /// <summary>
    /// Retrieves the AppData object from the file specified by the dataPath.
    /// </summary>
    /// <returns>
    /// The deserialized AppData object on success, or the default AppData object if 
    /// any error occurs, in which case a message is logged.
    /// </returns>
    public AppData GetAppData() =>
        DataPersistence
            .ReadFromFile(_dataPath)
            .TapError(_logger.CouldNotReadAppData)
            .GetValueOrDefault(_defaultData);

    /// <summary>
    /// Updates the provided AppData with a newer rate if available.
    /// </summary>
    /// <param name="data">The AppData object to potentially update.</param>
    /// <param name="mRate">A Maybe containing a Rate object. If the Maybe has no value, the method returns Maybe.None.</param>
    /// <returns>
    /// Maybe.None if the provided rate is not newer than the existing rate in the AppData object, 
    /// otherwise returns a Maybe containing the updated AppData object with the new rate.
    /// </returns>
    public Maybe<AppData> UpdateIfNewerRate([NotNull] AppData data, Maybe<Rate<Usd, Ves>> mRate)
    {
        if (mRate.HasNoValue)
        {
            return Maybe.None;
        }

        var rate = mRate.Value;
        if (data.LastRate is null || rate.IsNewerThan(data.LastRate))
        {
            _logger.UpdatingData();
            return Maybe.From(data.UpdatedWith(rate));
        }

        _logger.RateIsNotNew();
        return Maybe.None;
    }

    /// <summary>
    /// Stores the provided AppData object on the file specified by the dataPath.
    /// If an error occurred, a message is logged.
    /// </summary>
    /// <param name="data">The AppData object to be stored.</param>
    public void StoreAppData(AppData data) =>
        DataPersistence.WriteToFile(data, _dataPath).TapError(_logger.CouldNotWriteAppData);

}
