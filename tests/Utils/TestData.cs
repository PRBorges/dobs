using Dobs.Data.Types;
using RateProvider.Types;

namespace Dobs.Tests.Utils;

/// <summary>
/// Provides static test data used within Dobs unit tests.
/// </summary>
public static class TestData
{
    public static readonly Uri BcvUri =
        new("https://www.bcv.org.ve/estadisticas/tipo-cambio-de-referencia-smc");

    public static readonly TimeOnly RateChangeTimeUtc = new(19, 30);

    public static readonly AppData ExampleData =
        new(
            new Rate<Usd, Ves>(1.00000000m, new DateOnly(2024, 4, 1)),
            new Rate<Usd, Ves>(36.33400000m, new DateOnly(2024, 3, 27)),
            RateChangeTimeUtc,
            BcvUri
        );

    public static readonly AppData DefaultAppData = new AppData(
        null,
        null,
        RateChangeTimeUtc,
        BcvUri
    );
}
