using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Mcis;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class MCIListContentResponseDto : ListResponseDto<MCIContentResponseDto>
{
}
