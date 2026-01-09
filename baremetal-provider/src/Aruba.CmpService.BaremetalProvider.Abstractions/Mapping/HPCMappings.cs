using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.Extensions.Linq;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class HPCMappings
{
    public static HPCListResponseDto? MapToResponse([NotNull] this HPCList hpcs, HttpRequest httpRequest)
    {
        var ret = new HPCListResponseDto()
        {
            TotalCount = hpcs.TotalCount,
            Values = hpcs.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static HPCListContentResponseDto? MapToListResponse(this HPC hpc)
    {
        if (hpc == null)
        {
            return null;
        }
        var ret = new HPCListContentResponseDto();
        ret.Values = new List<HPCContentResponseDto>();
        hpc.Services.ForEach(s => ret.Values.Add(s.MapToResponse()));
        ret.TotalCount = hpc.Services.Count;

        return ret;
    }

    public static HPCResponseDto? MapToResponse(this HPC hpc)
    {
        if (hpc == null)
        {
            return null;
        }
        var ret = new HPCResponseDto();
        ret.Status = hpc.Status!.MapToResponse();
        ret.Metadata = hpc.MapMetadataToResponse();
        ret.Properties = hpc.Properties!.MapToResponse();

        ret.NumServices = hpc.NumServices;
        ret.MonthlyUnitPrice = hpc.MonthlyUnitPrice;

        hpc.Services.ForEach(s => ret.Services.Add(s.MapToResponse()!));

        return ret;
    }

    private static HPCPropertiesResponseDto? MapToResponse(this HPCProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new HPCPropertiesResponseDto()
        {
            ActivationDate = properties.ActivationDate,
            AutoRenewEnabled = properties.AutoRenewEnabled,
            AutoRenewDeviceId = properties.AutoRenewDeviceId,
            DueDate = properties.DueDate,
            Folders = properties.Folders,
            RenewAllowed = properties.RenewAllowed,
            ShowVat = properties.ShowVat,
        };

        return ret;
    }

    private static HPCContentResponseDto? MapToResponse(this HPCBundleContent service)
    {
        service.ThrowIfNull();
        var ret = new HPCContentResponseDto()
        {
            HPCServiceID = service.ServiceID,
            HPCServiceName = service.ServiceName,
            HPCServiceStatus = service.ServiceStatus,
            HPCServiceType = service.ServiceType,
            HPCServiceTypeCategory = service.ServiceTypeCategory,
        };

        return ret;
    }

    public static HPC MapToListitem(this LegacyHPCListItem hpc, string userId, Providers.Models.Projects.Project project, Location? location)
    {
        hpc.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new HPC()
        {
            Category = new Category()
            {
                Name = Categories.MetalCloudInfrastructure.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.HPC.Value,
                    Name = Typologies.HPC.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = hpc.ActivationDate?.ToDateTimeOffset(),
            Id = hpc.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = hpc.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = hpc.ActivationDate?.ToDateTimeOffset(),
                State = hpc.Status == HPCStatuses.Activating ? StatusValues.Activating.Value : hpc.Status == HPCStatuses.Active ? StatusValues.Active.Value : StatusValues.Suspended.Value,
            },
            UpdateDate = hpc.ActivationDate?.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = $"/projects/{project.Metadata.Id}/providers/Aruba.Baremetal/mcis/{hpc.Id}",
            Properties = hpc.MapListitemProperties(),
            NumServices = hpc.NumServices
        };

        return ret;
    }

    private static HPCProperties MapListitemProperties(this LegacyHPCListItem mci)
    {
        mci.ThrowIfNull();
        return new HPCProperties()
        {
            ActivationDate = mci.ActivationDate?.ToDateTimeOffset(),
            DueDate = mci.ExpirationDate?.ToDateTimeOffset(),
            Folders = mci.IncludedInFolders,
            Name = mci.Name,
        };
    }

    public static HPC MapToDetail(this LegacyHPCDetail hpcd, Location? location, Providers.Models.Projects.Project project, bool isResellerCustomer = false)
    {
        hpcd.ThrowIfNull();
        project.ThrowIfNull();

        var ret = new HPC()
        {
            Id = hpcd.Id.ToString(CultureInfo.InvariantCulture),
            Name = hpcd.Name,
            CreationDate = hpcd.ActivationDate.ToDateTimeOffset(),
            Status = new Status()
            {
                CreationDate = hpcd.ActivationDate,
                State = hpcd.Status == HPCStatuses.Active ? StatusValues.Active.Value : StatusValues.Suspended.Value,
            },
            Properties = hpcd.MapDetailsProperties(isResellerCustomer),
            MonthlyUnitPrice = hpcd.MonthlyUnitPrice,
            Location = location,
            Project = new Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            NumServices = hpcd.NumServices,
        };

        hpcd.BundleContent?.ForEach(s => ret.Services.Add(s.MapToDetail()));

        return ret;
    }

    private static HPCProperties MapDetailsProperties(this LegacyHPCDetail mcid, bool isResellerCustomer)
    {
        mcid.ThrowIfNull();
        return new HPCProperties()
        {
            ActivationDate = mcid.ActivationDate.ToDateTimeOffset(),
            DueDate = mcid.ExpirationDate,
            Folders = mcid.IncludedInFolders,
            Name = mcid.Name,
            AutoRenewEnabled = mcid.AutoRenewEnabled,
            AutoRenewDeviceId = mcid.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewAllowed = mcid.RenewAllowed,
            ShowVat = mcid.AutoRenewEnabled == true
                      && mcid.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
                      && isResellerCustomer == false,
        };
    }

    private static HPCBundleContent MapToDetail(this LegacyBundleService service)
    {
        service.ThrowIfNull();
        var ret = new HPCBundleContent()
        {
            ServiceID = service.ServiceID,
            ServiceName = service.ServiceName,
            ServiceStatus = service.ServiceStatus,
            ServiceType = service.ServiceType,
            ServiceTypeCategory = (BundleServiceTypeCategories)service.ServiceType!,
        };

        return ret;
    }
    public static BaremetalResource Map(this HPC hpc, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = hpc.Id,
            Uri = hpc.Uri,
            Name = hpc.Name,
            Location = new LocationData()
            {
                City = hpc.Location?.City,
                Name = hpc.Location?.Name,
                Code = hpc.Location?.Code,
                Country = hpc.Location?.Country,
                Value = hpc.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = hpc.Project?.Id,
            },
            Tags = hpc.Tags,
            CreationDate = hpc.CreationDate,
            UpdateDate = hpc.UpdateDate,
            CreatedBy = hpc.CreatedBy,
            UpdatedBy = hpc.UpdatedBy,
            LinkedResources = hpc.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = hpc.Status.State,
                CreationDate = hpc.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = hpc.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = hpc.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = hpc.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = hpc.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = hpc.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = hpc.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = hpc.Category?.Typology?.Id,
                    Name = hpc.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = hpc.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = hpc.Version?.Data?.Current ?? default,
                    Previous = hpc.Version?.Data?.Previous,
                }
            }
        };
    }

    public static IEnumerable<HPCCatalogItem> MapToHPCCatalog(this IEnumerable<InternalHPCCatalog> catalog, string? language, IEnumerable<HPCConfiguration> bundleConfigurationList)
    {
        catalog.ThrowIfNull();
        var catalogItems = new List<HPCCatalogItem>();

        if (!catalog.Any()) return catalogItems;

        if (string.IsNullOrWhiteSpace(language)) language = "it";

        catalog.ForEach(i =>
        {
            if (bundleConfigurationList.Where(c => c.BundleConfigurationCode!.Equals(i.BundleConfigurationCode!, StringComparison.OrdinalIgnoreCase)).Any())
            {
                var itemData = i.Data.Where(d => d.Language!.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                itemData.ThrowIfNull();

                var configurationPrice = bundleConfigurationList.Where(c => c.BundleConfigurationCode!.Equals(i.BundleConfigurationCode!, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Price ?? (decimal)0.0;

                var item = new HPCCatalogItem()
                {
                    IsSoldOut = false,
                    Category = i.Category,
                    Code = i.Model,
                    Name = i.ServerName,
                    Price = configurationPrice,
                    DiscountedPrice = configurationPrice,
                    Hdd = itemData.Hdd,
                    Ram = itemData.Ram,
                    Processor = itemData.Cpu,
                    ServerName = itemData.HardwareName,
                    NodeNumber = itemData.NodeNumber,
                    BundleConfigurationCode = i.BundleConfigurationCode,
                };

                var firewallData = i.Firewall.Data.Where(d => d.Language!.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                firewallData.ThrowIfNull();

                item.FirewallCode = i.Firewall.Model;
                item.FirewallName = firewallData.FirewallName;

                catalogItems.Add(item);

            }
        });

        return catalogItems;
    }

    public static HPCCatalogResponseDto? MapToResponse([NotNull] this Models.HPCs.HPCCatalog catalog)
    {
        var ret = new HPCCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };

        return ret;
    }

    public static HPCCatalogItemResponseDto? MapToResponse(this HPCCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new HPCCatalogItemResponseDto()
        {
            IsSoldOut = item.IsSoldOut,
            Code = item.Code,
            Category = item.Category,
            DiscountedPrice = item.DiscountedPrice,
            Price = item.Price,
            Processor = item.Processor,
            Hdd = item.Hdd,
            Name = item.Name,
            Ram = item.Ram,
            ServerName = item.ServerName,
            NodeNumber = item.NodeNumber,
            FirewallCode = item.FirewallCode,
            FirewallName = item.FirewallName,
            BundleConfigurationCode = item.BundleConfigurationCode,
        };

        return ret;
    }
}
