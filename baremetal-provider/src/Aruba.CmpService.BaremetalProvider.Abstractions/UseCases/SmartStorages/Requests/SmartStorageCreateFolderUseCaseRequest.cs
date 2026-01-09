using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
public class SmartStorageCreateFolderUseCaseRequest:BaseUserUseCaseRequest
{
    /// <summary>
    /// New folder name
    /// </summary>
    public string? Name { get; set; }

}
