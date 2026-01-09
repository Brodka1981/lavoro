using System.Text.Json.Serialization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

public class LegacyServiceWithRegion
{
    [JsonPropertyName("ServiceId")]
    public long Id { get; set; }

    [JsonPropertyName("ServiceName")]
    public string? Name { get; set; }

    [JsonPropertyName("Region")]
    public string? Region { get; set; }

    [JsonPropertyName("ServiceType")]
    public LegacyServiceType? ServiceType { get; set; }

    [JsonPropertyName("CustomServiceName")]
    public string? CustomName { get; set; }
}
