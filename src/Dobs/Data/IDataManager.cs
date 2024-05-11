using CSharpFunctionalExtensions;
using Dobs.Data.Types;
using RateProvider.Types;

namespace Dobs.Data;

/// <summary>
/// Defines an interface for managing app data.
/// </summary>
public interface IDataManager
{
    public AppData GetAppData();
    public Maybe<AppData> UpdateIfNewerRate(AppData data, Maybe<Rate<Usd, Ves>> mRate);
    public void StoreAppData(AppData data);
}
