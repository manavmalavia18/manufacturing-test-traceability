using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Data;
using MiniMes.Web.Domain;
using MiniMes.Web.Services;

namespace MiniMes.Tests;

public sealed class TestResultServiceTests
{
    [Fact]
    public async Task SubmitAsync_RecordsPassingUnit_WhenMeasurementsAreWithinLimits()
    {
        await using var db = CreateDatabase();
        var service = new TestResultService(db, TimeProvider.System);
        var submission = new TestSubmission(
            Guid.NewGuid(), "SQ-2026-0042", "FINAL-TEST-01", 28.1m, 42.3m, 1_240);

        var result = await service.SubmitAsync(submission);

        Assert.Equal(TestOutcome.Pass, result.Outcome);
        Assert.Null(result.FailureReason);
        Assert.Equal(1, await db.ManufacturingTests.CountAsync());
    }

    [Fact]
    public async Task SubmitAsync_RecordsFailureReason_WhenVoltageIsOutOfRange()
    {
        await using var db = CreateDatabase();
        var service = new TestResultService(db, TimeProvider.System);
        var submission = new TestSubmission(
            Guid.NewGuid(), "SQ-2026-0043", "FINAL-TEST-01", 30.2m, 44.0m, 1_100);

        var result = await service.SubmitAsync(submission);

        Assert.Equal(TestOutcome.Fail, result.Outcome);
        Assert.Contains("voltage", result.FailureReason!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitAsync_IsIdempotent_WhenStationRetriesSameEvent()
    {
        await using var db = CreateDatabase();
        var service = new TestResultService(db, TimeProvider.System);
        var eventId = Guid.NewGuid();
        var submission = new TestSubmission(
            eventId, "SQ-2026-0044", "FINAL-TEST-02", 27.9m, 39.8m, 980);

        var first = await service.SubmitAsync(submission);
        var retry = await service.SubmitAsync(submission);

        Assert.Equal(first.Id, retry.Id);
        Assert.Equal(1, await db.ManufacturingTests.CountAsync());
    }

    private static MiniMesDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<MiniMesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MiniMesDbContext(options);
    }
}
