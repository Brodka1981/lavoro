using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

// FIXME: @matteo.filippa 
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class HPCResponseDto : ResponseDto<HPCPropertiesResponseDto>
{
    public int? NumServices { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public List<HPCContentResponseDto> Services { get; set; } = new List<HPCContentResponseDto>();
}
