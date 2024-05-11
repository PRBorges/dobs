namespace Dobs.Tests.Utils.FileProvider;

/// <summary>
/// Provides methods for creating and managing temporary files for testing purposes.
/// </summary>
public static class TempFileProvider
{
    /// <summary>
    /// Private field storing the temporary directory used for creating temporary files.
    /// </summary>
    private static readonly DirectoryInfo _tempFolder = Directory.CreateTempSubdirectory(
        "TestFiles"
    );

    /// <summary>
    /// Gets the path to a non-existent file within the temporary directory.
    /// </summary>
    /// <returns>
    /// The path to a unique file that does not currently exist.
    /// </returns>
    public static string GetNonExistentFilePath()
    {
        string path;
        do
        {
            path = Path.Combine(_tempFolder.FullName, Path.GetRandomFileName());
        } while (File.Exists(path));
        return path;
    }

    /// <summary>
    /// Gets the path to an empty file within the temporary directory.
    /// </summary>
    /// <returns>
    /// The path to a newly created file with no content.
    /// </returns>
    public static string GetEmptyFilePath()
    {
        var filePath = GetNonExistentFilePath();
        File.WriteAllText(filePath, string.Empty);
        return filePath;
    }

    /// <summary>
    /// Creates a new temporary file with the specified content.
    /// </summary>
    /// <param name="content">The content to write to the temporary file.</param>
    /// <returns>
    /// The path to the newly created temporary file containing the provided content.
    /// </returns>
    public static string WithContent(string content)
    {
        var filePath = GetNonExistentFilePath();
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
