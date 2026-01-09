using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
public class SmartStorageFoldersRequest
{
    public string? ResourceId { get; set; }
    public string? UserId { get; set; }
    public string? ProjectId { get; set; }
}
