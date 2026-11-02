using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Data;
using MiniMes.Web.Domain;

namespace MiniMes.Web.Services;

public sealed class TraceabilityService(MiniMesDbContext db)
{
    public async Task<UnitGenealogy?> GetUnitGenealogyAsync(
        string serialNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = serialNumber.Trim().ToUpperInvariant();
        if (normalized.Length == 0)
        {
            return null;
        }

        var materials = await db.MaterialLots.AsNoTracking()
            .Where(x => x.SerialNumber == normalized)
            .OrderBy(x => x.Component)
            .ToListAsync(cancellationToken);

        var tests = await db.ManufacturingTests.AsNoTracking()
            .Where(x => x.SerialNumber == normalized)
            .OrderByDescending(x => x.RecordedAtUtc)
            .ToListAsync(cancellationToken);

        if (materials.Count == 0 && tests.Count == 0)
        {
            return null;
        }

        var suffix = normalized.Split('-').LastOrDefault() ?? "0000";
        return new UnitGenealogy(
            normalized,
            $"WO-48{suffix[^Math.Min(2, suffix.Length)..]}",
            "28 V DC-DC Power Module",
            materials,
            tests);
    }
}
