namespace MiniMes.Web.Domain;

public sealed class MaterialLot
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string LotCode { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
}

public sealed record UnitGenealogy(
    string SerialNumber,
    string WorkOrder,
    string ProductFamily,
    IReadOnlyList<MaterialLot> MaterialLots,
    IReadOnlyList<ManufacturingTest> TestHistory);
