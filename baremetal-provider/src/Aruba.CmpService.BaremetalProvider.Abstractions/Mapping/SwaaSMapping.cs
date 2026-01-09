using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class SwaasMapping
{
    #region Response
    public static SwaasListResponseDto? MapToResponse([NotNull] this SwaasList swaases, HttpRequest httpRequest)
    {
        var ret = new SwaasListResponseDto()
        {
            TotalCount = swaases.TotalCount,
            Values = swaases.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static SwaasResponseDto? MapToResponse(this Swaas swaas)
    {
        if (swaas == null)
        {
            return null;
        }
        var ret = new SwaasResponseDto();
        ret.Status = swaas.Status!.MapToResponse();
        ret.Metadata = swaas.MapMetadataToResponse();
        ret.Properties = swaas.Properties!.MapToResponse();
        return ret;
    }

    private static SwaasPropertiesResponseDto? MapToResponse(this SwaasProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new SwaasPropertiesResponseDto()
        {
            ActivationDate = properties.ActivationDate,
            Admin = properties.Admin,
            AutoRenewEnabled = properties.AutoRenewEnabled,
            AutoRenewAllowed = properties.AutoRenewAllowed,
            DueDate = properties.DueDate,
            Folders = properties.Folders,
            MonthlyUnitPrice = properties.MonthlyUnitPrice,
            Model = properties.Model,
            RenewAllowed = properties.RenewAllowed,
            Reply = properties.Reply,
            UpgradeAllowed = properties.UpgradeAllowed,
            RenewUpgradeAllowed = properties.RenewUpgradeAllowed,
            AutoRenewDeviceId = properties.AutoRenewDeviceId,
            RenewMonths = properties.RenewMonths,
            ShowVat = properties.ShowVat,
            MaxAvailability = properties.MaxAvailability,
        };
        return ret;
    }

    public static VirtualSwitchLinkListResponseDto? MapToResponse([NotNull] this VirtualSwitchLinkList virtualSwitchLinks, HttpRequest httpRequest)
    {
        var ret = new VirtualSwitchLinkListResponseDto()
        {
            TotalCount = virtualSwitchLinks.TotalCount,
            Values = virtualSwitchLinks.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static VirtualSwitchLinkResponseDto MapToResponse(this VirtualSwitchLink link)
    {
        var ret = new VirtualSwitchLinkResponseDto();

        ret.Id = link.Id;
        ret.LinkedServiceName = link.LinkedServiceName;
        ret.LinkedServiceTypology = link.LinkedServiceTypology;
        ret.VlanId = link.VlanId;
        ret.Status = link.Status;
        ret.VirtualSwitchId = link.VirtualSwitchId;
        ret.VirtualSwitchName = link.VirtualSwitchName;
        ret.LinkedServiceId = link.LinkedServiceId;

        return ret;
    }

    public static VirtualSwitchResponseDto? MapToResponse(this VirtualSwitch virtualSwitch)
    {
        if (virtualSwitch == null)
        {
            return null;
        }

        return new VirtualSwitchResponseDto()
        {
            Id = virtualSwitch.Id,
            Name = virtualSwitch.Name,
            Status = virtualSwitch.Status,
            Location = virtualSwitch.Location,
            LocationCodes = virtualSwitch.LocationCodes
        };
    }

    public static SwaasCatalogResponseDto? MapToResponse([NotNull] this Models.SwaaSes.SwaasCatalog catalog, HttpRequest httpRequest)
    {
        var ret = new SwaasCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static SwaasCatalogItemResponseDto? MapToResponse(this SwaasCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new SwaasCatalogItemResponseDto();
        ret.IsSoldOut = item.IsSoldOut;
        ret.Code = item.Code;
        ret.Category = item.Category;
        ret.SetupFeePrice = item.SetupFeePrice;
        ret.DiscountedPrice = item.DiscountedPrice;
        ret.Price = item.Price;
        ret.Name = item.Name;
        ret.LinkedDevicesCount = item.LinkedDevicesCount;
        ret.NetworksCount = item.NetworksCount;
        return ret;
    }

    public static VirtualSwitchListResponseDto? MapToResponse([NotNull] this VirtualSwitchList sirtualSwitches, HttpRequest httpRequest)
    {
        var ret = new VirtualSwitchListResponseDto()
        {
            TotalCount = sirtualSwitches.TotalCount,
            Values = sirtualSwitches.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static List<LinkableServiceResponseDto> MapToResponse(this List<LinkableService> linkableServices)
    {
        return linkableServices.Select(s => new LinkableServiceResponseDto()
        {
            Id = s.Id,
            Name = s.Name,
            Typology = s.Typology
        }).ToList();
    }

    #endregion

    #region Model
    public static VirtualSwitch Map(this LegacyVirtualSwitch legacyVirtualSwitches, IEnumerable<LegacyRegion> legacyRegions)
    {
        var region = legacyRegions.FirstOrDefault(r => r.RegionId?.Equals(legacyVirtualSwitches.Region, StringComparison.OrdinalIgnoreCase) ?? false);
        return new VirtualSwitch()
        {
            Id = legacyVirtualSwitches.VirtualNetworkId,
            Name = legacyVirtualSwitches.FriendlyName,
            Status = legacyVirtualSwitches.Status,
            Location = region?.RegionName,
        };
    }

    public static Swaas MapToListitem(this LegacySwaasListItem swaas, string userId, Providers.Models.Projects.Project project, Location? location)
    {
        swaas.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Swaas()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Swaas.Value,
                    Name = Typologies.Swaas.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = swaas.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = swaas.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = swaas.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = swaas.ActivationDate.ToDateTimeOffset(),
                State = swaas.Status.ToString()
            },
            UpdateDate = swaas.ActivationDate.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Swaas.CreateUri(project.Metadata.Id!, swaas.Id),
            Properties = swaas.MapListitemProperties()
        };
        return ret;
    }

    private static SwaasProperties MapListitemProperties(this LegacySwaasListItem swaas)
    {
        swaas.ThrowIfNull();
        return new SwaasProperties()
        {
            ActivationDate = swaas.ActivationDate.ToDateTimeOffset(),
            DueDate = swaas.ExpirationDate,
            Model = swaas.Model,
            Folders = swaas.IncludedInFolders ?? new List<string>()
        };
    }

    public static Swaas MapToDetail(this LegacySwaasDetail swaas, string userId, Providers.Models.Projects.Project project, Location? location, bool isResellerCustomer)
    {
        swaas.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Swaas()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Swaas.Value,
                    Name = Typologies.Swaas.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = swaas.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = swaas.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = string.IsNullOrWhiteSpace(swaas.CustomName) ? swaas.Name : swaas.CustomName,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = swaas.ActivationDate.ToDateTimeOffset(),
                State = swaas.Status.ToString()
            },
            UpdateDate = swaas.ActivationDate.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Swaas.CreateUri(project.Metadata.Id!, swaas.Id),
            Properties = swaas.MapDetailProperties(isResellerCustomer)
        };

        return ret;
    }

    private static SwaasProperties MapDetailProperties(this LegacySwaasDetail swaas, bool isResellerCustomer)
    {
        swaas.ThrowIfNull();
        return new SwaasProperties()
        {
            ActivationDate = swaas.ActivationDate,
            DueDate = swaas.ExpirationDate,
            Admin = swaas.Name,
            Model = swaas.Model,
            Reply = swaas.Reply,
            AutoRenewEnabled = swaas.AutoRenewEnabled,
            AutoRenewAllowed = swaas.AutoRenewAllowed,
            AutoRenewDeviceId = swaas.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewMonths = swaas.AutoRenewInfo?.RenewMonths ?? 0,
            Folders = swaas.IncludedInFolders ?? new List<string>(),
            UpgradeAllowed = swaas.UpgradeAllowed,
            RenewUpgradeAllowed = swaas.RenewUpgradeAllowed,
            MonthlyUnitPrice = swaas.MonthlyUnitPrice,
            RenewAllowed = swaas.RenewAllowed,
            MaxAvailability = swaas.MaxAvailability,
            ShowVat = swaas.AutoRenewEnabled == true
            && swaas.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false
        };
    }

    public static BaremetalResource Map(this Swaas swaas, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = swaas.Id,
            Uri = swaas.Uri,
            Name = swaas.Name,
            Location = new LocationData()
            {
                City = swaas.Location?.City,
                Name = swaas.Location?.Name,
                Code = swaas.Location?.Code,
                Country = swaas.Location?.Country,
                Value = swaas.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = swaas.Project?.Id,
            },
            Tags = swaas.Tags,
            CreationDate = swaas.CreationDate,
            UpdateDate = swaas.UpdateDate,
            CreatedBy = swaas.CreatedBy,
            UpdatedBy = swaas.UpdatedBy,
            LinkedResources = swaas.LinkedResources?.Select(r => new LinkedResourceData()
            {
                Id = r.Id,
                LinkCreationDate = r.LinkCreationDate,
                LinkType = r.LinkType,
                Ready = r.Ready,
                RelationName = r.RelationName,
                StrictCorrelation = r.StrictCorrelation,
                Typology = r.Typology,
                Uri = r.Uri,
            }).ToCollection() as Collection<LinkedResourceData> ?? new Collection<LinkedResourceData>(),
            Status = new StatusData()
            {
                State = swaas.Status.State,
                CreationDate = swaas.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = swaas.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = swaas.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = swaas.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = swaas.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = swaas.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = swaas.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = swaas.Category?.Typology?.Id,
                    Name = swaas.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = swaas.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = swaas.Version?.Data?.Current ?? default,
                    Previous = swaas.Version?.Data?.Previous,
                }
            }
        };
    }

    public static IEnumerable<SwaasCatalogItem> MapToSwaasCatalog(this IEnumerable<LegacyCatalogItem> legacyCatalogItems, IEnumerable<InternalSwaasCatalog> swaasCatalog, string? language)
    {
        legacyCatalogItems.ThrowIfNull();
        var swaasItems = new List<SwaasCatalogItem>();

        foreach (var legacyItem in legacyCatalogItems)
        {
            var catalogModel = swaasCatalog.FirstOrDefault(c => (c.Code?.Equals(legacyItem.ProductCode, StringComparison.OrdinalIgnoreCase) ?? false));

            var catalogData = catalogModel?.Data.FirstOrDefault(d => d.Language == language)
               ?? catalogModel?.Data.FirstOrDefault(d => d.Language == "it");

            swaasItems.Add(
                new SwaasCatalogItem
                {
                    IsSoldOut = legacyItem.IsSoldOut,
                    Code = legacyItem.ProductCode,
                    Price = legacyItem.Price,
                    Name = catalogData?.Model,
                    Category = legacyItem.Category,
                    SetupFeePrice = legacyItem.SetupFeePrice,
                    DiscountedPrice = legacyItem.DiscountedPrice,
                    NetworksCount = int.Parse(legacyItem.Features.FirstOrDefault(f => f.Code?.Equals("MAX_AVAILABLE_SW", StringComparison.OrdinalIgnoreCase) ?? false)?.Value ?? "0", CultureInfo.InvariantCulture),
                    LinkedDevicesCount = catalogModel?.LinkedDevicesCount,
                });
        }

        return swaasItems;
    }

    public static VirtualSwitchList MapToVirtualSwitchList(this IEnumerable<LegacyVirtualSwitch> legacyVirtualSwitches, IEnumerable<LegacyRegion> legacyRegions)
    {
        return new VirtualSwitchList()
        {
            TotalCount = legacyVirtualSwitches?.Count() ?? 0,
            Values = (legacyVirtualSwitches ?? new List<LegacyVirtualSwitch>()).Select(s => new VirtualSwitch
            {
                Id = s.VirtualNetworkId,
                Name = s.FriendlyName,
                Status = s.Status,                
                Location = legacyRegions.FirstOrDefault(r => r.RegionId!.Equals(s.Region, StringComparison.OrdinalIgnoreCase))?.RegionName,
                LocationCodes = s.Region?.Split("-")?.ToList() ?? new List<string>(),
                CreationDate = s.CreateDate,
            }).ToList()
        };
    }

    public static VirtualSwitch MapToVirtualSwitch(this LegacyVirtualSwitch virtualSwitch, IEnumerable<LegacyRegion> legacyRegions)
    {
        return new VirtualSwitch
        {
            Id = virtualSwitch.VirtualNetworkId,
            Name = virtualSwitch.FriendlyName,
            Status = virtualSwitch.Status,
            Location = legacyRegions.FirstOrDefault(r => r.RegionId!.Equals(virtualSwitch.Region, StringComparison.OrdinalIgnoreCase))?.RegionName,
            LocationCodes = virtualSwitch.Region?.Split("-")?.ToList() ?? new List<string>()
        };
    }

    public static VirtualSwitchLinkList MapToVirtualSwitchLinkList(this IEnumerable<LegacyVirtualSwitchLink> legacyVirtualSwitchLinks)
    {
        legacyVirtualSwitchLinks.ThrowIfNull();
        var list = legacyVirtualSwitchLinks.ToList();
        return new VirtualSwitchLinkList()
        {
            TotalCount = list.Count(),
            Values = list.Select(s => s.MapToVirtualSwitchLink()).ToList()
        };
    }


    public static VirtualSwitchLink MapToVirtualSwitchLink(this LegacyVirtualSwitchLink legacyVirtualSwitchLink)
    {
        legacyVirtualSwitchLink.ThrowIfNull();
        return new VirtualSwitchLink()
        {
            Id = legacyVirtualSwitchLink.Resource.ResourceId,
            Status = legacyVirtualSwitchLink.Resource.Status,
            VirtualSwitchId = legacyVirtualSwitchLink.VirtualNetwork.VirtualNetworkId,
            VirtualSwitchName = legacyVirtualSwitchLink.VirtualNetwork.FriendlyName,
            LinkedServiceId = legacyVirtualSwitchLink.Resource.ConnectedService.Id,
            LinkedServiceTypology = legacyVirtualSwitchLink.Resource.ConnectedService.ServiceType?.MapToCmpTypology()?.Value,
            LinkedServiceName = legacyVirtualSwitchLink.Resource.ConnectedServiceName,
            VlanId = legacyVirtualSwitchLink.Resource.VlanId?.ToString(),
            CreationDate = legacyVirtualSwitchLink.Resource.CreatedOn,
        };
    }
    #endregion
}

