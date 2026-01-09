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
public class SmartStorageCreateFolderUseCase : SmartStorageNoResponseBaseUseCase<SmartStorageCreateFolderUseCaseRequest, SmartStorageCreateFolderUseCaseResponse>
{
    public SmartStorageCreateFolderUseCase(ISmartStoragesService smartStoragesService)
        : base(smartStoragesService)
    {
        
    }
    protected override async Task<ServiceResult> ExecuteService(SmartStorageCreateFolderUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SmartStoragesService.CreateSmartFolder(request).ConfigureAwait(false);
        return ret;
    }
}
