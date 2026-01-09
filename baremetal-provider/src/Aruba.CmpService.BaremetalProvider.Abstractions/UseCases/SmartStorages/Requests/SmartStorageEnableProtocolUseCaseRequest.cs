using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;

public class SmartStorageEnableProtocolUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// Protocol type
    /// </summary>
    public ServiceType? ServiceType { get; set; }
}
