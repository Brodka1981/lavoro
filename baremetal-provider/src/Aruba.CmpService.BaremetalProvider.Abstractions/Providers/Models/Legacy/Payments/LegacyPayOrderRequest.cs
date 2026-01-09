using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyPayOrderRequest
{
    public string OrderId { get; set; }

    public int DeviceID { get; set; }

    public LegacyServiceType ServiceType { get; set; }

    public LegacyPaymentType DeviceType { get; set; }

    public object[] CustomInfo { get; set; } // FIXME: @martellata-hpc use proper model

    public string? RiskAssessmentOperationId { get; set; }

    public string? ReturnURL { get; set; }

    public string? PARes { get; set; }

    public string? ReferenceTransaction { get; set; }
}