namespace MiniMes.Web.Domain;

public enum TestOutcome
{
    Pass,
    Fail
}

public sealed record TestSubmission(
    Guid EventId,
    string SerialNumber,
    string Station,
    decimal OutputVoltage,
    decimal TemperatureC,
    int DurationMs);

public sealed class ManufacturingTest
{
    public int Id { get; set; }
    public Guid EventId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public decimal OutputVoltage { get; set; }
    public decimal TemperatureC { get; set; }
    public int DurationMs { get; set; }
    public TestOutcome Outcome { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
}

public sealed record DashboardSummary(
    int TotalTests,
    int Passed,
    int Failed,
    decimal PassRate);
