using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;

public abstract class BaremetalUpdatedDeployment : IResourceMessage
{
    public BaremetalResource? Resource { get; set; }
    public string? DeploymentId => Resource?.Id;
}
