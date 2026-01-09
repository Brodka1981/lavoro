using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Mcis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.Extensions.Linq;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class MCIMappings
{
    public static MCIListResponseDto? MapToResponse([NotNull] this MCIList mcis, HttpRequest httpRequest)
    {
        var ret = new MCIListResponseDto()
        {
            TotalCount = mcis.TotalCount,
            Values = mcis.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static MCIListContentResponseDto? MapToListResponse(this MCI mci)
    {
        if (mci == null)
        {
            return null;
        }
        var ret = new MCIListContentResponseDto();
        ret.Values = new List<MCIContentResponseDto>();
        mci.Services.ForEach(s => ret.Values.Add(s.MapToResponse()));
        ret.TotalCount = mci.Services.Count;

        return ret;
    }

    public static MCIResponseDto? MapToResponse(this MCI mci)
    {
        if (mci == null)
        {
            return null;
        }
        var ret = new MCIResponseDto();
        ret.Status = mci.Status!.MapToResponse();
        ret.Metadata = mci.MapMetadataToResponse();
        ret.Properties = mci.Properties!.MapToResponse();

        ret.NumServices = mci.NumServices;
        ret.MonthlyUnitPrice = mci.MonthlyUnitPrice;

        mci.Services.ForEach(s => ret.Services.Add(s.MapToResponse()!));

        return ret;
    }

    private static MCIPropertiesResponseDto? MapToResponse(this MCIProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new MCIPropertiesResponseDto()
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

    private static MCIContentResponseDto? MapToResponse(this BundleContent service)
    {
        service.ThrowIfNull();
        var ret = new MCIContentResponseDto()
        {
            MCIServiceID = service.ServiceID,
            MCIServiceName = service.ServiceName,
            MCIServiceStatus = service.ServiceStatus,
            MCIServiceType = service.ServiceType,
            MCIServiceTypeCategory = service.ServiceTypeCategory,
        };

        return ret;
    }

    public static MCI MapToListitem(this LegacyMCIListItem mci, string userId, Providers.Models.Projects.Project project, Location? location)
    {
        mci.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new MCI()
        {
            Category = new Category()
            {
                Name = Categories.MetalCloudInfrastructure.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.MCI.Value,
                    Name = Typologies.MCI.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = mci.ActivationDate?.ToDateTimeOffset(),
            Id = mci.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = mci.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = mci.ActivationDate?.ToDateTimeOffset(),
                State = mci.Status == MCIStatuses.Activating ? StatusValues.Activating.Value : mci.Status == MCIStatuses.Active ? StatusValues.Active.Value : StatusValues.Suspended.Value,
            },
            UpdateDate = mci.ActivationDate?.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = $"/projects/{project.Metadata.Id}/providers/Aruba.Baremetal/mcis/{mci.Id}",
            Properties = mci.MapListitemProperties(),
            NumServices = mci.NumServices
        };

        return ret;
    }

    private static MCIProperties MapListitemProperties(this LegacyMCIListItem mci)
    {
        mci.ThrowIfNull();
        return new MCIProperties()
        {
            ActivationDate = mci.ActivationDate?.ToDateTimeOffset(),
            DueDate = mci.ExpirationDate?.ToDateTimeOffset(),
            Folders = mci.IncludedInFolders,
            Name = mci.Name,
        };
    }

    public static MCI MapToDetail(this LegacyMCIDetail mcid, Location? location, Providers.Models.Projects.Project project, bool isResellerCustomer = false)
    {
        mcid.ThrowIfNull();
        project.ThrowIfNull();

        var ret = new MCI()
        {
            Id = mcid.Id.ToString(CultureInfo.InvariantCulture),
            Name = mcid.Name,
            CreationDate = mcid.ActivationDate.ToDateTimeOffset(),
            Status = new Status()
            {
                CreationDate = mcid.ActivationDate,
                State = mcid.Status == MCIStatuses.Active ? StatusValues.Active.Value : StatusValues.Suspended.Value,
            },
            Properties = mcid.MapDetailsProperties(isResellerCustomer),
            MonthlyUnitPrice = mcid.MonthlyUnitPrice,
            Location = location,
            Project = new Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            NumServices = mcid.NumServices,
        };

        mcid.BundleContent?.ForEach(s => ret.Services.Add(s.MapToDetail()));
        
        return ret;
    }

    private static MCIProperties MapDetailsProperties(this LegacyMCIDetail mcid, bool isResellerCustomer)
    {
        mcid.ThrowIfNull();
        return new MCIProperties()
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

    private static BundleContent MapToDetail(this LegacyBundleService service)
    {
        service.ThrowIfNull();
        var ret = new BundleContent()
        {
            ServiceID = service.ServiceID,
            ServiceName = service.ServiceName,
            ServiceStatus = service.ServiceStatus,
            ServiceType = service.ServiceType,
            ServiceTypeCategory = (BundleServiceTypeCategories)service.ServiceType!,
        };

        return ret;
    }
    public static BaremetalResource Map(this MCI mci, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = mci.Id,
            Uri = mci.Uri,
            Name = mci.Name,
            Location = new LocationData()
            {
                City = mci.Location?.City,
                Name = mci.Location?.Name,
                Code = mci.Location?.Code,
                Country = mci.Location?.Country,
                Value = mci.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = mci.Project?.Id,
            },
            Tags = mci.Tags,
            CreationDate = mci.CreationDate,
            UpdateDate = mci.UpdateDate,
            CreatedBy = mci.CreatedBy,
            UpdatedBy = mci.UpdatedBy,
            LinkedResources = mci.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = mci.Status.State,
                CreationDate = mci.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = mci.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = mci.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = mci.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = mci.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = mci.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = mci.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = mci.Category?.Typology?.Id,
                    Name = mci.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = mci.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = mci.Version?.Data?.Current ?? default,
                    Previous = mci.Version?.Data?.Previous,
                }
            }
        };
    }

    public static IEnumerable<MCICatalogItem> MapToMCICatalog(this IEnumerable<InternalMCICatalog> catalog, string? language, IEnumerable<MCIConfiguration> bundleConfigurationList) 
    {
        catalog.ThrowIfNull();
        var catalogItems = new List<MCICatalogItem>();

        if(!catalog.Any()) return catalogItems;

        if(string.IsNullOrWhiteSpace(language)) language = "it";

        catalog.ForEach(i =>
        {
            if(bundleConfigurationList.Where(c => c.BundleConfigurationCode!.Equals(i.BundleConfigurationCode!, StringComparison.OrdinalIgnoreCase)).Any())
            {
                var itemData = i.Data.Where(d => d.Language!.Equals(language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                itemData.ThrowIfNull();

                var configurationPrice = bundleConfigurationList.Where(c => c.BundleConfigurationCode!.Equals(i.BundleConfigurationCode!, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Price ?? (decimal)0.0;

                var item = new MCICatalogItem()
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

    public static MCICatalogResponseDto? MapToResponse([NotNull] this Models.MCIs.MCICatalog catalog)
    {
        var ret = new MCICatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };

        return ret;
    }

    public static MCICatalogItemResponseDto? MapToResponse(this MCICatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new MCICatalogItemResponseDto()
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
