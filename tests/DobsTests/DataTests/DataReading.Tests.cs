using CSharpFunctionalExtensions;
using Dobs.Data;
using Dobs.Tests.Utils.FileProvider;
using FluentAssertions;

namespace Dobs.Tests.DataTests;

/// <summary>
/// Unit tests for DataPersistence class' reading AppData.
/// </summary>
[Collection("TempFileProvider Clients")]
public class DataReadingTest
{
    private readonly TempFileProvider _tempFileProvider;
    private readonly string _readErrorStart = "Problem reading AppData";

    public DataReadingTest(TempFileProvider tempFileProvider)
    {
        _tempFileProvider = tempFileProvider;
    }

    [Theory]
    [MemberData(nameof(DataJsonProducer.CorrectJson), MemberType = typeof(DataJsonProducer))]
    public void CorrectFileShouldReadOk(string json)
    {
        var filePath = _tempFileProvider.WithContent(json);
        var rData = DataPersistence.ReadFromFile(filePath);

        rData.Should().Succeed("AppData json file is correct.");
    }

    [Fact]
    public void ReadFromNonExistentFileShouldFails()
    {
        var filePath = _tempFileProvider.GetNonExistentFilePath();

        var rData = DataPersistence.ReadFromFile(filePath);

        rData.Should().Fail("File to read AppData from does not exists");
        rData.TapError(error => error.Should().StartWith(_readErrorStart));
    }

    [Fact]
    public void ReadEmptyFileShouldFail()
    {
        var filePath = _tempFileProvider.GetEmptyFilePath();

        var rData = DataPersistence.ReadFromFile(filePath);

        rData.Should().Fail("File to read AppData from is empty.");
        rData.TapError(error => error.Should().StartWith(_readErrorStart));
    }

    [Theory]
    [MemberData(nameof(DataJsonProducer.WrongJson), MemberType = typeof(DataJsonProducer))]
    public void ReadFileWithWrongDataShouldFail(string json)
    {
        var filePath = _tempFileProvider.WithContent(json);

        var rData = DataPersistence.ReadFromFile(filePath);

        rData.Should().Fail("Json in AppData file is incorrect.");
        rData.TapError(error => error.Should().StartWith(_readErrorStart));
    }
}

public static class DataJsonProducer
{
    public static IEnumerable<object[]> CorrectJson =>
        EmbeddedFileReader.ReadAllWithSubstring("appData").Select(s => new object[] { s });

    public static IEnumerable<object[]> WrongJson =>
        EmbeddedFileReader.ReadAllWithSubstring("wrongAppData").Select(s => new object[] { s });
}
