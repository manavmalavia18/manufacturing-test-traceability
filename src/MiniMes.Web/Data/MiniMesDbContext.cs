using Microsoft.EntityFrameworkCore;
using MiniMes.Web.Domain;

namespace MiniMes.Web.Data;

public sealed class MiniMesDbContext(DbContextOptions<MiniMesDbContext> options)
    : DbContext(options)
{
    public DbSet<ManufacturingTest> ManufacturingTests => Set<ManufacturingTest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var test = modelBuilder.Entity<ManufacturingTest>();
        test.HasKey(x => x.Id);
        test.HasIndex(x => x.EventId).IsUnique();
        test.HasIndex(x => x.SerialNumber);
        test.Property(x => x.SerialNumber).HasMaxLength(64).IsRequired();
        test.Property(x => x.Station).HasMaxLength(64).IsRequired();
        test.Property(x => x.OutputVoltage).HasPrecision(8, 3);
        test.Property(x => x.TemperatureC).HasPrecision(8, 2);
        test.Property(x => x.FailureReason).HasMaxLength(256);
    }
}
