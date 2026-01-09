using System.Diagnostics.CodeAnalysis;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Ecommerce
{
    /// <summary>
    /// SubscriptionServices
    /// </summary>
    public SubscriptionServices? SubscriptionServices { get; set; }

    /// <summary>
    /// XProject
    /// </summary>
    public string? XProject { get; set; }
}
