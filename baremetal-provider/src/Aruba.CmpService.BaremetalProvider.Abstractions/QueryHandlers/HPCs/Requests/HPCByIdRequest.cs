using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
public class HPCByIdRequest :
    BaseGetByIdRequest<HPC>
{
    public bool CalculatePrices { get; set; }
}
