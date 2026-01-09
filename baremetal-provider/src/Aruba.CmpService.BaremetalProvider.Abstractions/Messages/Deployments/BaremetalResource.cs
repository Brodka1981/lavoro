using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;

public class BaremetalResource : ResourceBaseData
{
    public AutorenewFolderAction? Action { get; set; }
}
