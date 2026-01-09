using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyFraudRiskAssessmentResponse
{
    public bool Result { get; set; }
}
