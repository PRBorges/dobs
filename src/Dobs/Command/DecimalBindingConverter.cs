using System.Globalization;
using CliFx.Extensibility;

namespace Dobs;

/// <summary>
/// Implements the CliFx.BindingConverter for the Amount argument.
/// </summary>
public class DecimalBindingConverter : BindingConverter<decimal>
{
    /// <summary>
    /// Parsers a string to a decimal using CurrentCulture. Allows 
    /// leading sign and decimal separator, but no thousands separator.
    /// </summary>
    /// <param name="rawValue">The string containing the decimal value to be converted.</param>
    /// <returns>
    /// The converted decimal value, or the default decimal value if the input string is null, whitespace, or cannot be parsed.
    /// </returns>
    public override decimal Convert(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return default;
        }
        return decimal.Parse(
            rawValue,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.CurrentCulture
        );
    }
}
