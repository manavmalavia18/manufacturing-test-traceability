using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Domain;
using MiniMes.Web.Services;

namespace MiniMes.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MiniMesDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (await db.ManufacturingTests.AnyAsync())
        {
            return;
        }

        var service = scope.ServiceProvider.GetRequiredService<TestResultService>();
        var samples = new[]
        {
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1042", "FINAL-TEST-01", 28.10m, 42.3m, 1240),
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1043", "FINAL-TEST-02", 27.82m, 39.7m, 1015),
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1044", "FINAL-TEST-01", 30.16m, 45.8m, 1189),
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1045", "BURN-IN-03", 28.04m, 71.4m, 1440),
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1046", "FINAL-TEST-02", 28.21m, 41.1m, 970),
            new TestSubmission(Guid.NewGuid(), "SQ-2026-1047", "FINAL-TEST-01", 27.94m, 43.0m, 1088)
        };

        foreach (var sample in samples)
        {
            await service.SubmitAsync(sample);
        }
    }
}
