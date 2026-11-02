using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Data;
using MiniMes.Web.Domain;
using MiniMes.Web.Services;

namespace MiniMes.Tests;

public sealed class TraceabilityServiceTests
{
    [Fact]
    public async Task GetUnitGenealogyAsync_ReturnsMaterialLotsAndTestHistory()
    {
        await using var db = CreateDatabase();
        db.ManufacturingTests.Add(new ManufacturingTest
        {
            EventId = Guid.NewGuid(),
            SerialNumber = "SQ-2026-1044",
            Station = "FINAL-TEST-01",
            OutputVoltage = 30.16m,
            TemperatureC = 45.8m,
            DurationMs = 1189,
            Outcome = TestOutcome.Fail,
            FailureReason = "Voltage outside limit.",
            RecordedAtUtc = DateTimeOffset.UtcNow
        });
        db.MaterialLots.AddRange(
            new MaterialLot
            {
                SerialNumber = "SQ-2026-1044",
                Component = "Power transformer",
                PartNumber = "MAG-28V-004",
                LotCode = "MAG-261005-B",
                Supplier = "Approved Supplier A",
                ReceivedDate = new DateOnly(2026, 10, 5)
            },
            new MaterialLot
            {
                SerialNumber = "SQ-2026-1044",
                Component = "Control PCB",
                PartNumber = "PCB-CTRL-210",
                LotCode = "PCB-260930-A",
                Supplier = "Internal Assembly",
                ReceivedDate = new DateOnly(2026, 9, 30)
            });
        await db.SaveChangesAsync();
        var service = new TraceabilityService(db);

        var genealogy = await service.GetUnitGenealogyAsync(" sq-2026-1044 ");

        Assert.NotNull(genealogy);
        Assert.Equal("SQ-2026-1044", genealogy.SerialNumber);
        Assert.Equal(2, genealogy.MaterialLots.Count);
        Assert.Single(genealogy.TestHistory);
        Assert.Equal(TestOutcome.Fail, genealogy.TestHistory[0].Outcome);
    }

    [Fact]
    public async Task GetUnitGenealogyAsync_ReturnsNull_WhenSerialDoesNotExist()
    {
        await using var db = CreateDatabase();
        var service = new TraceabilityService(db);

        var genealogy = await service.GetUnitGenealogyAsync("SQ-UNKNOWN");

        Assert.Null(genealogy);
    }

    private static MiniMesDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<MiniMesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MiniMesDbContext(options);
    }
}
