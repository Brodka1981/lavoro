namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

public class InternalSmartStorageCatalog
{
    public string? Code { get; set; }

    public string? Size { get; set; }

    public string? Snapshot { get; set; }

    public bool Replica { get; set; }
}
