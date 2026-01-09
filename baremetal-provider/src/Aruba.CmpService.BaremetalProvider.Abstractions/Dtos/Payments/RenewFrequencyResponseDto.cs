using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class RenewFrequencyResponseDto :
    List<byte>
{
    public RenewFrequencyResponseDto(IEnumerable<byte> months)
    {
        this.AddRange(months.OrderBy(o => o).ToList());
    }
}
