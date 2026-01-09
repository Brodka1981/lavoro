using System.Collections.ObjectModel;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.CmpService.ResourceProvider.Common.Messages.v1;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Aruba.MessageBus.Models;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class CommonMapping
{
    public static IpAddressStatuses Map(this LegacyIpAddressStatuses status)
    {
        switch (status)
        {
            case LegacyIpAddressStatuses.Active:
                return IpAddressStatuses.Active;
            case LegacyIpAddressStatuses.InActive:
                return IpAddressStatuses.Inactive;
            case LegacyIpAddressStatuses.ToBeActivated:
                return IpAddressStatuses.Updating;
            default:
                throw new ArgumentOutOfRangeException("Invalid legacy status");
        }
    }
    public static UpdateIpAddress Map(this UpdateIpUseCaseRequest request)
    {
        request.ThrowIfNull();
        var ret = new UpdateIpAddress();
        ret.IpAddressId = request.Id;
        ret.CustomName = request.IpAddress?.Description;
        ret.Hosts = request.IpAddress?.HostNames ?? new Collection<string>();
        return ret;
    }

    public static LegacySearchFilters Map(this ServerIpAddressesFilterRequest request)
    {
        var ret = new LegacySearchFilters();
        ret.External = request?.External ?? false;
        ret.Query = request?.Query ?? new ResourceQueryDefinition(new ResourceQueryDefinitionOptions());
        return ret;
    }
    public static LegacySearchFilters Map(this CatalogFilterRequest request)
    {
        var ret = new LegacySearchFilters();
        ret.External = request?.External ?? false;
        ret.Query = request?.Query ?? new ResourceQueryDefinition(new ResourceQueryDefinitionOptions());
        return ret;
    }

    public static StatusResponseDto MapToResponse(this Status status)
    {
        var ret = new StatusResponseDto();
        ret.State = status?.State;
        ret.CreationDate = status?.CreationDate;
        return ret;
    }

    public static CategoryResponseDto MapToResponse(this Category category)
    {
        var ret = new CategoryResponseDto();
        ret.Provider = category?.Provider;
        ret.Name = category?.Name;
        ret.Typology = category?.Typology?.MapToResponse();
        return ret;
    }

    public static TypologyResponseDto MapToResponse(this Typology typology)
    {
        var ret = new TypologyResponseDto();
        ret.Name = typology?.Name;
        ret.Id = typology?.Id;
        return ret;
    }

    public static LocationResponseDto? MapToResponse(this Location? location)
    {
        if (location == null)
        {
            return null;
        }
        var ret = new LocationResponseDto();
        ret.Name = location.Name;
        ret.Country = location.Country;
        ret.Value = location.Value;
        ret.Code = location.Code;
        ret.City = location.City;
        return ret;
    }

    public static ProjectResponseDto? MapToResponse(this Models.Project? project)
    {
        if (project == null)
        {
            return null;
        }
        var ret = new ProjectResponseDto();
        ret.Id = project.Id;
        return ret;
    }

    public static MetadataResponseDto MapMetadataToResponse<T>(this T resource) where T : IResourceBase
    {
        if (resource == null)
        {
            return null;
        }
        var ret = new MetadataResponseDto();

        ret.Category = resource!.Category!.MapToResponse();
        ret.CreatedBy = resource.CreatedBy;
        ret.UpdatedBy = resource.CreatedBy;
        ret.Id = resource.Id;
        ret.CreationDate = resource.CreationDate;
        ret.Ecommerce = null;
        ret.Name = resource.Name;
        ret.Version = resource.Version?.Data?.Current == null ? "1" : resource.Version.Data.Current.ToString(CultureInfo.InvariantCulture);
        ret.UpdateDate = resource.UpdateDate;
        ret.Uri = resource.Uri;
        ret.Location = resource.Location.MapToResponse();
        ret.Project = resource.Project?.MapToResponse();

        return ret;
    }
    internal static IpAddressTypes MapIpAddressType(this int ipAddressType)
    {
        switch (ipAddressType)
        {
            case 0:
                return IpAddressTypes.Primary;
            case 1:
                return IpAddressTypes.Management;
            case 2:
                return IpAddressTypes.Additional;
            default:
                throw new ArgumentOutOfRangeException($"Invalid FirewallIpAddressTypes {ipAddressType}");
        }
    }

    public static Envelope CreateEnvelopeForDeploymentStatusChanged(this BaseUserUseCaseRequest request, Typologies typology)
    {
        var deploymentStatusChanged = new DeploymentStatusChanged();
        deploymentStatusChanged.Status = new StatusData()
        {
            CreationDate = DateTimeOffset.UtcNow,
            State = StatusValues.Active.Value
        };
        deploymentStatusChanged.CreatedBy = request?.UserId;
        deploymentStatusChanged.DeploymentId = request?.ResourceId;
        deploymentStatusChanged.Typology = typology?.Create().MapToData();

        return new EnvelopeBuilder().WithSubject(request?.UserId ?? request!.ResourceId)
            .Build(deploymentStatusChanged);
    }



    public static Envelope CreateEnvelopeForDeploymentStatusChanged(this BaseUserUseCaseRequest request, string deploymentId, string typologyId)
    {
        var deploymentStatusChanged = new DeploymentStatusChanged();
        deploymentStatusChanged.Status = new StatusData()
        {
            CreationDate = DateTimeOffset.UtcNow,
            State = StatusValues.Active.Value
        };
        deploymentStatusChanged.CreatedBy = request?.UserId;
        deploymentStatusChanged.DeploymentId = deploymentId;
        deploymentStatusChanged.Typology = new TypologyData()
        {
            Id = typologyId,
            Name = typologyId?.ToUpperInvariant()
        };

        return new EnvelopeBuilder().WithSubject(request?.UserId ?? request!.ResourceId)
            .Build(deploymentStatusChanged);
    }
    public static DeploymentStatusChanged MapToDeploymentStatusChanged<T>(this T resource) where T : IResourceBase
    {
        var ret = new DeploymentStatusChanged();
        ret.Status = resource!.Status?.MapToData();
        ret.CreatedBy = resource.CreatedBy;
        ret.DeploymentId = resource.Id;
        ret.Typology = resource.Category!.Typology!.MapToData();

        return ret;
    }

    private static Typology Create(this Typologies typology)
    {
        return new Typology()
        {
            Id = typology.Value,
            Name = typology.Value.ToUpperInvariant()
        };
    }

    private static TypologyData MapToData(this Typology typology)
    {
        return new TypologyData()
        {
            Id = typology.Id,
            Name = typology.Name,
        };
    }

    private static StatusData MapToData(this Status status)
    {
        return new StatusData()
        {
            CreationDate = status.CreationDate,
            DisableStatusInfo = status.DisableStatusInfo?.MapToData(),
            State = status.State,
        };
    }

    private static DisableStatusInfoData MapToData(this DisableStatusInfo disableStausInfo)
    {
        var ret = new DisableStatusInfoData();
        ret.IsDisabled = disableStausInfo.IsDisabled;
        ret.PreviousStatus = disableStausInfo.PreviousStatus?.MapToData();
        ret.Reasons.AddRange(disableStausInfo.Reasons.Select(s => s.Reason).Where(w => !string.IsNullOrWhiteSpace(w)).OfType<string>().ToCollection());

        return ret;
    }

    private static PreviousStatusData MapToData(this PreviousStatus previousStatus)
    {
        var ret = new PreviousStatusData();
        ret.State = previousStatus.State;
        ret.CreationDate = previousStatus.CreationDate;

        return ret;
    }

    internal static string CreateUri(this Typologies typology, string projectId, long id)
    {
        var stringType = "";
        switch (typology.Value)
        {
            case var type when type == Typologies.Firewall.Value:
                stringType = "firewalls";
                break;
            case var type when type == Typologies.Server.Value:
                stringType = "servers";
                break;
            case var type when type == Typologies.SmartStorage.Value:
                stringType = "smartstorages";
                break;
            case var type when type == Typologies.Swaas.Value:
                stringType = "swaas";
                break;
            case var type when type == Typologies.Switch.Value:
                stringType = "switches";
                break;
            default:
                throw new ArgumentOutOfRangeException("Invalid typology");
        }
        return $"/projects/{projectId}/providers/Aruba.Baremetal/{stringType}/{id}";
    }

    /// <summary>
    /// Map typology to legacy typology value
    /// </summary>
    public static LegacyServiceType MapToLegacyTypology(this string typology)
    {
        switch (typology)
        {
            case var type when type == Typologies.Server.Value:
                return LegacyServiceType.Server;

            case var type when type == Typologies.Switch.Value:
                return LegacyServiceType.Switch;

            case var type when type == Typologies.Firewall.Value:
                return LegacyServiceType.Firewall;

            case var type when type == Typologies.SmartStorage.Value:
                return LegacyServiceType.SmartStorage;

            case var type when type == Typologies.Swaas.Value:
                return LegacyServiceType.SwitchSWAAS;

            default:
                return LegacyServiceType.None;
        }
    }


    /// <summary>
    /// Map typology to legacy typology value
    /// </summary>
    public static Typologies MapToCmpTypology(this LegacyServiceType legacySeviceType)
    {
        switch (legacySeviceType)
        {
            case LegacyServiceType.Server:
                return Typologies.Server;

            case LegacyServiceType.Switch:
                return Typologies.Switch;

            case LegacyServiceType.Firewall:
                return Typologies.Firewall;

            case LegacyServiceType.SmartStorage:
                return Typologies.SmartStorage;

            case LegacyServiceType.SwitchSWAAS:
                return Typologies.Swaas;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
