using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers
{
    public interface IProfileProvider
    {
        Task<ApiCallOutput<UserProfile>> GetUser(string userId);
    }
}
