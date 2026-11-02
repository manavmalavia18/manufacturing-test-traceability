using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Data;
using MiniMes.Web.Domain;

namespace MiniMes.Web.Services;

public sealed class TestResultService(MiniMesDbContext db, TimeProvider clock)
{
    private const decimal MinimumVoltage = 27.0m;
    private const decimal MaximumVoltage = 29.0m;
    private const decimal MaximumTemperatureC = 70.0m;

    public async Task<ManufacturingTest> SubmitAsync(
        TestSubmission submission,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.ManufacturingTests
            .SingleOrDefaultAsync(x => x.EventId == submission.EventId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        Validate(submission);
        var failureReason = EvaluateFailure(submission);
        var test = new ManufacturingTest
        {
            EventId = submission.EventId,
            SerialNumber = submission.SerialNumber.Trim().ToUpperInvariant(),
            Station = submission.Station.Trim().ToUpperInvariant(),
            OutputVoltage = submission.OutputVoltage,
            TemperatureC = submission.TemperatureC,
            DurationMs = submission.DurationMs,
            Outcome = failureReason is null ? TestOutcome.Pass : TestOutcome.Fail,
            FailureReason = failureReason,
            RecordedAtUtc = clock.GetUtcNow()
        };

        db.ManufacturingTests.Add(test);
        await db.SaveChangesAsync(cancellationToken);
        return test;
    }

    public Task<List<ManufacturingTest>> GetRecentAsync(
        int count = 20,
        CancellationToken cancellationToken = default) =>
        db.ManufacturingTests.AsNoTracking()
            .OrderByDescending(x => x.RecordedAtUtc)
            .Take(Math.Clamp(count, 1, 100))
            .ToListAsync(cancellationToken);

    public Task<List<ManufacturingTest>> GetHistoryAsync(
        string serialNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = serialNumber.Trim().ToUpperInvariant();
        return db.ManufacturingTests.AsNoTracking()
            .Where(x => x.SerialNumber == normalized)
            .OrderByDescending(x => x.RecordedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<DashboardSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var total = await db.ManufacturingTests.CountAsync(cancellationToken);
        var passed = await db.ManufacturingTests
            .CountAsync(x => x.Outcome == TestOutcome.Pass, cancellationToken);
        var failed = total - passed;
        var passRate = total == 0 ? 0 : decimal.Round((decimal)passed / total * 100, 1);
        return new DashboardSummary(total, passed, failed, passRate);
    }

    private static void Validate(TestSubmission submission)
    {
        if (submission.EventId == Guid.Empty)
            throw new ArgumentException("Event ID is required.", nameof(submission));
        if (string.IsNullOrWhiteSpace(submission.SerialNumber))
            throw new ArgumentException("Serial number is required.", nameof(submission));
        if (string.IsNullOrWhiteSpace(submission.Station))
            throw new ArgumentException("Station is required.", nameof(submission));
        if (submission.DurationMs <= 0)
            throw new ArgumentException("Duration must be positive.", nameof(submission));
    }

    private static string? EvaluateFailure(TestSubmission submission)
    {
        if (submission.OutputVoltage < MinimumVoltage || submission.OutputVoltage > MaximumVoltage)
            return $"Output voltage {submission.OutputVoltage:F2} V is outside the 27.00–29.00 V limit.";
        if (submission.TemperatureC > MaximumTemperatureC)
            return $"Temperature {submission.TemperatureC:F1} °C exceeds the 70.0 °C limit.";
        return null;
    }
}
