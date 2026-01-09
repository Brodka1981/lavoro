using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface ILegacyProvider<TResourceListItem, TResourceDetail>
    where TResourceListItem : LegacyResourceListItem
    where TResourceDetail : LegacyResourceDetail
{
    Task<ApiCallOutput<TResourceDetail>> GetById(long id);
    Task<ApiCallOutput<LegacyListResponse<TResourceListItem>>> Search(LegacySearchFilters filterRequest);
    Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename);
    Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew);
    Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew);
    Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog();    
}
