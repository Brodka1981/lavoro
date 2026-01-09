using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface IProjectProvider
{
    /// <summary>
    /// Get default project for the given user
    /// </summary>
    Task<ApiCallOutput<Project?>> GetDefaultProjectAsync(string userId);
    Task<ApiCallOutput<Project?>> GetProjectAsync(string userId, string id);
}
