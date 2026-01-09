using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyServerListItem :
    LegacyResourceListItem
{
    public ServerStatuses? Status { get; set; }
    public string? Model { get; set; }
    public string? CPU { get; set; }
    public string? GPU { get; set; }
    public string? RAM { get; set; }
    public string? OS { get; set; }
    public string? HDD { get; set; }
    public string? IpAddress { get; set; }
    public string? ModelTypeCode { get; set; }
    public string? ServerFarmCode { get; set; }
    public DateTime ActivationDate { get; set; }
}

