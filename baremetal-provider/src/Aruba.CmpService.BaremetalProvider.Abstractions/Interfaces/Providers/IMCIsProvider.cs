using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface IMCIsProvider :
    ILegacyProvider<LegacyMCIListItem, LegacyMCIDetail>
{
    Task<ApiCallOutput<LegacyMCIDetail>> GetContentById(long id, LegacySearchFilters filterRequest);
    Task<ApiCallOutput<LegacyMCIDetail>> GetByIdWithPrices(long id, bool calculatePrices);
    Task<ApiCallOutput<List<MCIConfiguration>>> GetMCICatalog();
}
