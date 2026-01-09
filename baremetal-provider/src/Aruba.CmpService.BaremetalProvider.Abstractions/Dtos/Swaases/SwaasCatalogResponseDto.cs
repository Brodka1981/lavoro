using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SwaasCatalogResponseDto : ListResponseDto<SwaasCatalogItemResponseDto>
{
}
