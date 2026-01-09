using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class InternalAutorechargeUseCaseRequest : BaseUserUseCaseRequest
{
    /// <summary>
    /// If TRUE use <see cref="IAdminLegacyProvider"/> with admin token
    /// </summary>
    public bool UseAdminProvider { get; set; }
}
