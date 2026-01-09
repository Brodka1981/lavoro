using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyProtocol
{
    public LegacyServiceType ServiceType { get; set; }
    public LegacyServiceStatus ServiceStatus { get; set; }

    /// <summary>
    /// TRUE if an error occured while retrieving service data
    /// </summary>
    public bool Error { get; set; }

    /// <summary>
    /// Description of the error occured while retrieving service data
    /// </summary>
    public string? ErrorMessage { get; set; }

}
