using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SubscriptionExtraInfo
{
    /// <summary>
    /// Provisioning Product Id 
    /// </summary>
    public string? ProvisioningProductId { get; set; }
    
    /// <summary>
    /// Provisioning Product Name 
    /// </summary>
    public string? ProvisioningProductName { get; set; }
    
    /// <summary>
    /// Data center
    /// </summary>
    public string? DataCenter { get; set; }
}
