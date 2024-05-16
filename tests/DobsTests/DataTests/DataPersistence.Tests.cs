using Dobs.Data;
using Dobs.Data.Types;
using Dobs.Tests.Utils;
using Dobs.Tests.Utils.FileProvider;
using FluentAssertions;
using RateProvider.Types;

namespace Dobs.Tests.DataTests;

/// <summary>
/// Tests for the DataPersistence class.
/// </summary>
[Collection("TempFileProvider Clients")]
public class DataPersistenceTests
{
    private readonly TempFileProvider _tempFileProvider;

    public DataPersistenceTests(TempFileProvider tempFileProvider)
    {
        _tempFileProvider = tempFileProvider;
    }

    [Theory]
    [MemberData(nameof(AppDataProducer.Data), MemberType = typeof(AppDataProducer))]
    public void WritesAndReadsDataBackOk(AppData data)
    {
        var filePath = _tempFileProvider.GetNonExistentFilePath();
        DataPersistence.WriteToFile(data, filePath).Should().Succeed();

        var rDataFromFile = DataPersistence.ReadFromFile(filePath);
        rDataFromFile
            .Should()
            .SucceedWith(data, "AppData should deserialize and be read from file");
    }
}

public static class AppDataProducer
{
    private static List<(Rate<Usd, Ves>?, Rate<Usd, Ves>?)> _dataRates =>
        new List<(Rate<Usd, Ves>?, Rate<Usd, Ves>?)>
        {
            (null, null),
            (null, new Rate<Usd, Ves>(35m, DateOnly.FromDateTime(DateTime.Now))),
            (new Rate<Usd, Ves>(100.3456m, new DateOnly(2024, 12, 31)), null),
            (
                new Rate<Usd, Ves>(5m, DateOnly.FromDateTime(DateTime.Now)),
                new Rate<Usd, Ves>(109.3456789m, new DateOnly(2024, 12, 31))
            ),
        };

    public static IEnumerable<object[]> Data =>
        _dataRates.Select(rs => new object[] { dataWithRates(rs) });

    private static readonly Func<(Rate<Usd, Ves>? r1, Rate<Usd, Ves>? r2), AppData> dataWithRates =
        rs => new AppData(rs.r1, rs.r2, TestData.RateChangeTimeUtc, TestData.BcvUri);
}
