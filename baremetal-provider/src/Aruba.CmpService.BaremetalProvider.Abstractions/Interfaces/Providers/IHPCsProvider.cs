using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface IHPCsProvider :
    ILegacyProvider<LegacyHPCListItem, LegacyHPCDetail>
{
    Task<ApiCallOutput<LegacyHPCDetail>> GetContentById(long id, LegacySearchFilters filterRequest);
    Task<ApiCallOutput<LegacyHPCDetail>> GetByIdWithPrices(long id, bool calculatePrices);
    Task<ApiCallOutput<List<HPCConfiguration>>> GetHPCCatalog();
}
