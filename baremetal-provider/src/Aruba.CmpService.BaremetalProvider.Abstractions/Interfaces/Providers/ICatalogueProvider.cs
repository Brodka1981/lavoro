using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface ICatalogueProvider
{
    Task<ApiCallOutput<Location>> GetLocationByValue(string value);
}
