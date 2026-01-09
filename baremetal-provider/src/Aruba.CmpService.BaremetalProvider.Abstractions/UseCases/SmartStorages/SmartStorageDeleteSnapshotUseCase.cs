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
public class SmartStorageDeleteSnapshotUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageDeleteSnapshotUseCaseRequest, SmartStorageDeleteSnapshotUseCaseResponse>
{
    public SmartStorageDeleteSnapshotUseCase(ISmartStoragesService smartStoragesService)
        :base(smartStoragesService)
    {
        
    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageDeleteSnapshotUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.DeleteSnapshot(request).ConfigureAwait(false);
        return ret;
    }
}
