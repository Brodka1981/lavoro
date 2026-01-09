using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
public class LegacyResource
{
    public int Id { get; set; }
    public LegacyServiceType ServiceType { get; set; }
    public string? Name { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public int? AutoRenewFrequency { get; set; }
    public decimal? Price { get; set; }
    public DateTime? ExpiringDate { get; set; }
    public long? DeviceId { get; set; }
    public LegacyPaymentType? DeviceType { get; set; }
    public string? Status { get; set; }

}
