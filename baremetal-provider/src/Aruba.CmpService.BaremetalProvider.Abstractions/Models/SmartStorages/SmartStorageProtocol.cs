using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SmartStorageProtocol
{
    public ServiceType ServiceType { get; set; }
    public ServiceStatus ServiceStatus { get; set; }
    public bool Error { get; set; }
    public string? ErrorMessage { get; set; }
}
