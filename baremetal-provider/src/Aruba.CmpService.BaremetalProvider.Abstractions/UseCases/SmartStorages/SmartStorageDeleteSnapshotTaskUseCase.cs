using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
public class SmartStorageDeleteSnapshotTaskUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageDeleteSnapshotTaskUseCaseRequest, SmartStorageDeleteSnapshotTaskUseCaseResponse>
{
    public SmartStorageDeleteSnapshotTaskUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {
        
    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageDeleteSnapshotTaskUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.DeleteSnapshotTask(request).ConfigureAwait(false);
        return ret;
    }
}
