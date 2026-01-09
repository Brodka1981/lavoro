using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Mcis;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class MCIResponseDto : ResponseDto<MCIPropertiesResponseDto>
{
    public int? NumServices { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public List<MCIContentResponseDto> Services { get; set; } = new List<MCIContentResponseDto>();
}
