namespace Dobs.Data;

/// <summary>
/// Encapsulates static BCV data: the web page uri and the rate change time.
/// </summary>
public static class BcvParams
{
    /// <summary>
    /// Approximate UTC time at which the next business day rate are
    /// published at the BCV site.
    /// </summary>
    public static readonly TimeOnly RateChangeTimeUtc = new TimeOnly(19, 30);

    /// <summary>
    /// Uri of the BCV web page where rates are published.
    /// </summary>
    public static readonly Uri BcvUri = new Uri("https://www.bcv.org.ve/estadisticas/tipo-cambio-de-referencia-smc");
}
