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

        var serials = samples.Select(x => x.SerialNumber).ToArray();
        foreach (var serial in serials)
        {
            db.MaterialLots.AddRange(
                CreateLot(serial, "Control PCB", "PCB-CTRL-210", "PCB-260930-A", "Internal Assembly", new DateOnly(2026, 9, 30)),
                CreateLot(serial, "Power transformer", "MAG-28V-004", "MAG-261005-B", "Approved Supplier A", new DateOnly(2026, 10, 5)),
                CreateLot(serial, "Power MOSFET", "FET-100V-017", "FET-261014-7", "Approved Supplier B", new DateOnly(2026, 10, 14)));
        }

        await db.SaveChangesAsync();
    }

    private static MaterialLot CreateLot(
        string serialNumber,
        string component,
        string partNumber,
        string lotCode,
        string supplier,
        DateOnly receivedDate) => new()
        {
            SerialNumber = serialNumber,
            Component = component,
            PartNumber = partNumber,
            LotCode = lotCode,
            Supplier = supplier,
            ReceivedDate = receivedDate
        };
}
