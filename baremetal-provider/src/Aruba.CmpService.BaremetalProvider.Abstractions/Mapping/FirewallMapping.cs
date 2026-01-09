using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class FirewallMapping
{
    public static LegacySearchFilters Map(this FirewallIpAddressesFilterRequest request)
    {
        var ret = new LegacySearchFilters();
        ret.External = request?.External ?? false;
        ret.Query = request?.Query ?? new ResourceQueryDefinition(new ResourceQueryDefinitionOptions());
        return ret;
    }
    public static LegacySearchFilters Map(this FirewallVlanIdFilterRequest request)
    {
        var ret = new LegacySearchFilters();
        ret.External = request?.External ?? false;
        ret.Query = request?.Query ?? new ResourceQueryDefinition(new ResourceQueryDefinitionOptions());
        return ret;
    }
    #region Response
    public static FirewallCatalogResponseDto? MapToResponse([NotNull] this FirewallCatalog catalog, HttpRequest httpRequest)
    {
        var ret = new FirewallCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static FirewallCatalogItemResponseDto? MapToResponse(this FirewallCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new FirewallCatalogItemResponseDto();
        ret.IsSoldOut = item.IsSoldOut;
        ret.Code = item.Code;
        ret.Category = item.Category;
        ret.SetupFeePrice = item.SetupFeePrice;
        ret.DiscountedPrice = item.DiscountedPrice;
        ret.Price = item.Price;
        ret.Mode = item.Mode;
        ret.Location = item.Location;
        ret.Name = item.Name;
        return ret;
    }
    public static FirewallListResponseDto? MapToResponse([NotNull] this FirewallList firewalls, HttpRequest httpRequest)
    {
        var ret = new FirewallListResponseDto()
        {
            TotalCount = firewalls.TotalCount,
            Values = firewalls.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static FirewallResponseDto? MapToResponse(this Firewall firewall)
    {
        if (firewall == null)
        {
            return null;
        }
        var ret = new FirewallResponseDto();
        ret.Status = firewall.Status!.MapToResponse();
        ret.Metadata = firewall.MapMetadataToResponse();
        ret.Properties = firewall.Properties!.MapToResponse();
        return ret;
    }

    private static FirewallPropertiesResponseDto? MapToResponse(this FirewallProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new FirewallPropertiesResponseDto()
        {
            ActivationDate = properties.ActivationDate,
            Admin = properties.Admin,
            AutoRenewEnabled = properties.AutoRenewEnabled,
            AutoRenewAllowed = properties.AutoRenewAllowed,
            AutoRenewDeviceId = properties.AutoRenewDeviceId,
            RenewMonths = properties.RenewMonths,
            DueDate = properties.DueDate,
            Folders = properties.Folders,
            IpAddress = properties.IpAddress,
            ConfigurationMode = properties.ConfigurationMode,
            MonthlyUnitPrice = properties.MonthlyUnitPrice,
            ShowVat = properties.ShowVat,
            RenewAllowed = properties.RenewAllowed,
            Model = properties.Model,
            BundleCode = properties.BundleCode,
            BundleProjectName = properties.BundleProjectName,
        };
        return ret;
    }

    public static FirewallIpAddressListResponseDto? MapToResponse([NotNull] this FirewallIpAddressList ipAddresses, HttpRequest httpRequest)
    {
        var ret = new FirewallIpAddressListResponseDto()
        {
            TotalCount = ipAddresses.TotalCount,
            Values = ipAddresses.Values.Select(s => s.MapToFirewallResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static FirewallIpAddressResponseDto? MapToFirewallResponse(this IpAddress ipAddress)
    {
        ipAddress.ThrowIfNull();
        var ret = new FirewallIpAddressResponseDto();
        ret.Id = ipAddress.Id;
        ret.Ip = ipAddress.Ip;
        ret.HostNames.AddRange(ipAddress.HostNames ?? new Collection<string>());
        ret.Description = ipAddress.Description;
        ret.Status = ipAddress.Status;
        ret.Type = ipAddress.Type;

        return ret;
    }

    public static FirewallVlanIdListResponseDto? MapToVlanIdResponse([NotNull] this FirewallVlanIdList vlanIds, HttpRequest httpRequest)
    {
        var ret = new FirewallVlanIdListResponseDto()
        {
            TotalCount = vlanIds.TotalCount,
            Values = new List<FirewallVlanIdResponseDto>()
        };

        if (vlanIds.TotalCount > 0) ret.Values = vlanIds.Values.Select(s => s.MapToFirewallVlanIdResponse()!).ToList();
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static FirewallVlanIdResponseDto? MapToFirewallVlanIdResponse(this VlanId vlanid)
    {
        var ret = new FirewallVlanIdResponseDto();
        ret.Vlanid = vlanid?.Vlanid;

        return ret;
    }
    #endregion

    #region ToModel
    public static IEnumerable<FirewallCatalogItem> MapToFirewallCatalog(
        this IEnumerable<LegacyCatalogItem> legacyCatalogItem,
        IEnumerable<InternalFirewallCatalog> firewallCatalog)
    {
        legacyCatalogItem.ThrowIfNull();
        var firewallItems = new List<FirewallCatalogItem>();

        foreach (var legacyItem in legacyCatalogItem)
        {
            var catalogData = firewallCatalog.FirstOrDefault(s => s.Code == legacyItem.ProductCode);
            firewallItems.Add(new FirewallCatalogItem()
            {
                IsSoldOut = legacyItem.IsSoldOut,
                Code = legacyItem.ProductCode,
                Price = legacyItem.Price,
                Name = legacyItem.DisplayName,
                Category = legacyItem.Category,
                Location = catalogData?.Location,
                SetupFeePrice = legacyItem.SetupFeePrice,
                DiscountedPrice = legacyItem.DiscountedPrice,
                Mode = catalogData?.Mode,
            });
        }

        return firewallItems;
    }

    public static FirewallIpAddressList MapToFirewallIpAddressList(this LegacyListResponse<LegacyIpAddress> ipAddresses)
    {
        ipAddresses.ThrowIfNull();
        return new FirewallIpAddressList
        {
            TotalCount = ipAddresses.TotalItems,
            Values = ipAddresses!.Items?.Select(s => new IpAddress()
            {
                Id = s.IpAddressId,
                Description = s.CustomName,
                Status = s.Status.Map(),
                HostNames = s.Hosts ?? new List<string>(),
                Ip = s.IpAddress,
                Type = s.IpType.MapIpAddressType()
            }).ToList() ?? new List<IpAddress>()
        };
    }

    public static FirewallVlanIdList MapToFirewallVlanIdList(this LegacyListResponse<LegacyVlanId> VlanIds)
    {
        return new FirewallVlanIdList
        {
            TotalCount = VlanIds?.TotalItems ?? 0,
            Values = VlanIds?.Items?.Select(s => new VlanId()
            {
                Vlanid = s.Vlanid,
            }).ToList() ?? new List<VlanId>()
        };
    }

    public static Firewall MapToListitem(this LegacyFirewallListItem firewall, string userId, Providers.Models.Projects.Project project, Location? location)
    {
        firewall.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Firewall()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Firewall.Value,
                    Name = Typologies.Firewall.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = firewall.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = firewall.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = firewall.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = firewall.ActivationDate,
                State = firewall.Status.ToString()
            },
            UpdateDate = firewall.ActivationDate,
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Firewall.CreateUri(project.Metadata.Id!, firewall.Id),
            Properties = firewall.MapListitemProperties()
        };

        return ret;
    }

    private static FirewallProperties MapListitemProperties(this LegacyFirewallListItem firewall)
    {
        firewall.ThrowIfNull();
        return new FirewallProperties()
        {
            ActivationDate = firewall.ActivationDate.ToDateTimeOffset(),
            DueDate = firewall.ExpirationDate,
            Admin = firewall.Name,
            Model = firewall.Model,
            ConfigurationMode = firewall.ConfigurationMode,
            Folders = firewall.IncludedInFolders ?? new List<string>(),
        };
    }

    public static Firewall MapToDetail(this LegacyFirewallDetail firewall, string userId, Providers.Models.Projects.Project project, Location? location, bool isResellerCustomer)
    {
        firewall.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Firewall()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Firewall.Value,
                    Name = Typologies.Firewall.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = firewall.ActivationDate,
            Ecommerce = null,
            Id = firewall.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = string.IsNullOrWhiteSpace(firewall.CustomName) ? firewall.Name : firewall.CustomName,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = firewall.ActivationDate,
                State = firewall.Status.ToString()
            },
            UpdateDate = firewall.ActivationDate,
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Firewall.CreateUri(project.Metadata.Id!, firewall.Id),
            Properties = firewall.MapDetailProperties(isResellerCustomer)
        };

        return ret;
    }

    private static FirewallProperties MapDetailProperties(this LegacyFirewallDetail firewall, bool isResellerCustomer)
    {
        firewall.ThrowIfNull();
        return new FirewallProperties()
        {
            ActivationDate = firewall.ActivationDate,
            DueDate = firewall.ExpirationDate,
            Admin = firewall.Name,
            Model = firewall.Model,
            AutoRenewEnabled = firewall.AutoRenewEnabled,
            AutoRenewAllowed = firewall.AutoRenewAllowed,
            AutoRenewDeviceId = firewall.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewMonths = firewall.AutoRenewInfo?.RenewMonths,
            ConfigurationMode = firewall.ConfigurationMode,
            Folders = firewall.IncludedInFolders ?? new List<string>(),
            IpAddress = firewall.IpAddress,
            MonthlyUnitPrice = firewall.MonthlyUnitPrice,
            RenewAllowed = firewall.RenewAllowed,
            ShowVat = firewall.AutoRenewEnabled == true
            && firewall.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false,
            BundleCode = firewall.BundleCode,
            BundleProjectName = firewall.BundleProjectName,
        };
    }

    public static BaremetalResource Map(this Firewall firewall, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = firewall.Id,
            Uri = firewall.Uri,
            Name = firewall.Name,
            Location = new LocationData()
            {
                City = firewall.Location?.City,
                Name = firewall.Location?.Name,
                Code = firewall.Location?.Code,
                Country = firewall.Location?.Country,
                Value = firewall.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = firewall.Project?.Id,
            },
            Tags = firewall.Tags,
            CreationDate = firewall.CreationDate,
            UpdateDate = firewall.UpdateDate,
            CreatedBy = firewall.CreatedBy,
            UpdatedBy = firewall.UpdatedBy,
            LinkedResources = firewall.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = firewall.Status.State,
                CreationDate = firewall.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = firewall.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = firewall.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = firewall.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = firewall.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = firewall.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = firewall.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = firewall.Category?.Typology?.Id,
                    Name = firewall.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = firewall.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = firewall.Version?.Data?.Current ??default,
                    Previous = firewall.Version?.Data?.Previous,
                }
            }
        };
    }
    #endregion
}