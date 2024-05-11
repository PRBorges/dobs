using System.Text.Json;
using CSharpFunctionalExtensions;
using Dobs.Data.Types;

namespace Dobs.Data;

/// <summary>
/// Provides methods for serializing and deserializing AppData objects to and from files.
/// </summary>
public static class DataPersistence
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { WriteIndented = true };

    /// <summary>
    /// Serializes an AppData object to JSON and writes it to the specified file.
    /// </summary>
    /// <param name="data">The AppData object to be written.</param>
    /// <param name="path">The full path to the file where the data will be written.</param>
    /// <returns>
    /// A UnitResult indicating success or failure. 
    /// On failure, the result will contain an error message describing the problem encountered.
    /// </returns>
    public static UnitResult<string> WriteToFile(AppData data, string path)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            File.WriteAllText(path, jsonString);
            return Result.Success();
        }
        catch (Exception ex)
            when (ex
                    is ArgumentException
                        or ArgumentNullException
                        or IOException
                        or UnauthorizedAccessException
                        or NotSupportedException
                        or System.Security.SecurityException
            )
        {
            return Result.Failure($"Problem writing to file {path}: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and deserializes an AppData object from the specified JSON file path.
    /// </summary>
    /// <param name="path">The full path to the file containing the AppData object.</param>
    /// <returns>
    /// A Result containing the deserialized AppData object on success, or a failure message if deserialization fails.
    /// </returns>
    public static Result<AppData> ReadFromFile(string path)
    {
        try
        {
            var jsonString = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<AppData>(jsonString);
            return data is null
                ? Result.Failure<AppData>($"Could not deserialize AppData from {path}")
                : Result.Success(data);
        }
        catch (Exception ex)
            when (ex
                    is ArgumentException
                        or ArgumentNullException
                        or IOException
                        or UnauthorizedAccessException
                        or NotSupportedException
                        or System.Security.SecurityException
                        or JsonException
            )
        {
            return Result.Failure<AppData>($"Problem reading AppData: {ex.Message}");
        }
    }
}
