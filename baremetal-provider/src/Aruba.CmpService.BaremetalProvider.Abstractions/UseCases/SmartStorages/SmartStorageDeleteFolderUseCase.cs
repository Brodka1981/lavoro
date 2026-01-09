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
public class SmartStorageDeleteFolderUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageDeleteFolderUseCaseRequest, SmartStorageDeleteFolderUseCaseResponse>
{
    public SmartStorageDeleteFolderUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {

    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageDeleteFolderUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.DeleteSmartFolder(request).ConfigureAwait(false);
        return ret;
    }
}
