using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Dobs.Tests.Utils.FileProvider;

/// <summary>
/// Provides methods for reading embedded resources within the executing assembly.
/// </summary>
public static class EmbeddedFileReader
{
    private static readonly Assembly _asm = Assembly.GetExecutingAssembly();

    private static IEnumerable<string> _resourceNames => _asm.GetManifestResourceNames();

    /// <summary>
    /// Reads an embedded resource as a string.
    /// </summary>
    /// <param name="fileName">The filename of the embedded resource.</param>
    /// <returns>The content of the resource as a string, or null if not found.</returns>
    public static string? ReadAsString(string fileName)
    {
        var resourceName = GetResourceName(fileName);
        return resourceName is null ? null : GetStreamAsString(resourceName);
    }

    /// <summary>
    /// Reads as strings all embedded resources with a name containing the given substring.
    /// </summary>
    /// <param name="subStr">The substring contained in the names of the resources to be read.</param>
    /// <returns>An enumeration of strings representing the content of each resource.</returns>
    public static IEnumerable<string> ReadAllWithSubstring(string subStr) =>
        _resourceNames
            .Where(name => name.Contains(subStr, StringComparison.CurrentCulture))
            .Select(GetStreamAsString)
            .AsEnumerable();

    /// <summary>
    /// Gets the full resource name of an embedded file.
    /// </summary>
    /// <param name="fileName">The filename of the embedded resource.</param>
    /// <returns>The full name of the resource, or null if not found.</returns>
    private static string? GetResourceName(string fileName) =>
        _resourceNames.FirstOrDefault(name =>
            name.EndsWith(fileName, StringComparison.CurrentCulture)
        );

    /// <summary>
    /// Reads an embedded resource as a string.
    /// </summary>
    /// <param name="resourceName">The name of the embedded resource.</param>
    /// <returns>The content of the resource as a string, or null if not found.</returns>
    private static string GetStreamAsString([NotNull] string resourceName)
    {
        using (var stream = _asm.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream!))
        {
            return reader.ReadToEnd();
        }
    }
}
