using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Dobs.Data.Types;
using Dobs.Data;
using Dobs.Tests.Utils.FileProvider;
using Dobs.Tests.Utils;
using CSharpFunctionalExtensions;
using RateProvider.Types;

namespace Dobs.Tests.DataTests;

/// <summary>
/// Unit tests for the DataManager class.
/// </summary>
public class DataManagerTest
{
    private static readonly AppData _exampleAppData = TestData.ExampleData;
    private static readonly AppData _defaultAppData = TestData.DefaultAppData;

    // GetAppData Tests

    [Fact]
    public void GetAppDataReturnsDefaultIfFileDoesNotExist()
    {
        var filePath = TempFileProvider.GetNonExistentFilePath();
        var dataManager = new DataManager(NullLogger.Instance, filePath);

        var appData = dataManager.GetAppData();

        appData.Should().Be(_defaultAppData, "AppData json file does not exists.");
    }

    [SkippableFact]
    public void GetAppDataReturnsDefaultIfFileIsWrong()
    {
        var json = EmbeddedFileReader.ReadAllWithSubstring("wrongAppData").FirstOrDefault();
        Skip.If(json is null, "No wrong AppData file found.");
        var filePath = TempFileProvider.WithContent(json);
        var dataManager = new DataManager(NullLogger.Instance, filePath);

        var appData = dataManager.GetAppData();

        appData.Should().Be(_defaultAppData, "AppData json is wrong.");
    }

    [SkippableFact]
    public void GetAppDataReadsCorrectFileOk()
    {
        var json = EmbeddedFileReader.ReadAsString("appData.example.json");
        Skip.If(json is null, "Embedded file example.json not found.");
        var filePath = TempFileProvider.WithContent(json);
        var dataManager = new DataManager(NullLogger.Instance, filePath);

        var appData = dataManager.GetAppData();

        appData
            .Should()
            .Be(_exampleAppData, "embedded.appData.other.json should ve read correctly.");
    }

    // UpdateIfNewerRate tests

    [Fact]
    public void DataIsNotUpdatedIfNoRate()
    {
        var dataManager = new DataManager(NullLogger.Instance, string.Empty);
        var noRate = Maybe<Rate<Usd, Ves>>.None;

        var mNewData = dataManager.UpdateIfNewerRate(_exampleAppData, noRate);

        mNewData.Should().HaveNoValue("No new app data if no rate.");
    }

    [Fact]
    public void DataShouldNotBeUpdatedIfNoLastRate()
    {
        var dataManager = new DataManager(NullLogger.Instance, string.Empty);
        var oldRate = new Rate<Usd, Ves>(1.5m, DateOnly.MinValue);
        var expectedNewData = _defaultAppData.UpdatedWith(oldRate);

        var mNewData = dataManager.UpdateIfNewerRate(_defaultAppData, Maybe.From(oldRate));

        mNewData
            .Should()
            .HaveValue(expectedNewData, "Default AttData should be updated with any rate.");
    }

    [Fact]
    public void DataShouldBeUpdatedWithNewRate()
    {
        var dataManager = new DataManager(NullLogger.Instance, string.Empty);
        var newRate = new Rate<Usd, Ves>(2.0m, DateOnly.FromDateTime(DateTime.Now));
        var expectedNewData = _exampleAppData.UpdatedWith(newRate);

        var mNewData = dataManager.UpdateIfNewerRate(_exampleAppData, Maybe.From(newRate));

        mNewData
            .Should()
            .HaveValue(expectedNewData, "Example data should be updated with today's rate.");
    }

    // StoreAppData tests

    [Fact]
    public void StoreAppDataShouldSaveInFile()
    {
        var filePath = TempFileProvider.GetNonExistentFilePath();
        var dataManager = new DataManager(NullLogger.Instance, filePath);

        dataManager.StoreAppData(_exampleAppData);

        File.Exists(filePath).Should().BeTrue();
    }
}
