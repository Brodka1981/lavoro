
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
public class PayOrderRequest
{
    public string OrderId { get; set; }

    public LegacyServiceType ServiceType { get; set; }
}
