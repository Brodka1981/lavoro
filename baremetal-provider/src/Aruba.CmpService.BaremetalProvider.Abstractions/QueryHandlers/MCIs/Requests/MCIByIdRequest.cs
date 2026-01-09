using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
public class MCIByIdRequest :
    BaseGetByIdRequest<MCI>
{
    public bool CalculatePrices { get; set; }
}
