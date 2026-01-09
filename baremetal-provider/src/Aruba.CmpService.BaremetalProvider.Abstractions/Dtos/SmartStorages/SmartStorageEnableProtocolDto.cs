using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Serialization;
using Aruba.CmpService.ResourceProvider.Common.Swagger;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageEnableProtocolDto
{
    [SwaggerRequired]
    [EnumJsonConverter]
    public ServiceType? ServiceType { get; set; }
}
