using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class ServerMappings
{
    #region ToResponse
    public static ServerListResponseDto? MapToResponse([NotNull] this ServerList servers, HttpRequest httpRequest)
    {
        var ret = new ServerListResponseDto()
        {
            TotalCount = servers.TotalCount,
            Values = servers.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static ServerResponseDto? MapToResponse(this Server server)
    {
        if (server == null)
        {
            return null;
        }
        var ret = new ServerResponseDto();
        ret.Status = server.Status!.MapToResponse();
        ret.Metadata = server.MapMetadataToResponse();
        ret.Properties = server.Properties!.MapToResponse();
        return ret;
    }
    public static ServerCatalogResponseDto? MapToResponse([NotNull] this Models.Servers.ServerCatalog catalog, HttpRequest httpRequest)
    {
        var ret = new ServerCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static ServerCatalogItemResponseDto? MapToResponse(this ServerCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new ServerCatalogItemResponseDto();
        ret.IsSoldOut = item.IsSoldOut;
        ret.Code = item.Code;
        ret.Category = item.Category;
        ret.SetupFeePrice = item.SetupFeePrice;
        ret.DiscountedPrice = item.DiscountedPrice;
        ret.Price = item.Price;
        ret.Processor = item.Processor;
        ret.Connectivity = item.Connectivity;
        ret.Hdd = item.Hdd;
        ret.Location = item.Location;
        ret.Name = item.Name;
        ret.Ram = item.Ram;
        ret.ServerName = item.ServerName;
        return ret;
    }

    private static ServerPropertiesResponseDto? MapToResponse(this ServerProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new ServerPropertiesResponseDto();

        ret.IpAddress = properties.IpAddress;
        ret.ActivationDate = properties.ActivationDate;
        ret.OperatingSystem = properties.OperatingSystem;
        ret.DueDate = properties.DueDate;
        ret.Hdd = properties.Hdd;
        ret.Gpu = properties.Gpu;
        ret.PleskLicense = properties.PleskLicense?.MapToResponse();
        ret.Processor = properties.Processor;
        ret.Model = properties.Model;
        ret.Ram = properties.Ram;
        ret.Folders = properties.Folders ?? new List<string>();
        ret.MonthlyUnitPrice = properties.MonthlyUnitPrice;
        ret.ShowVat = properties.ShowVat;
        ret.Components = properties.Components.Select(s => s.MapToResponse()).ToList();
        ret.AutoRenewEnabled = properties.AutoRenewEnabled;
        ret.AutoRenewAllowed = properties.AutoRenewAllowed;
        ret.RenewAllowed = properties.RenewAllowed;
        ret.UpgradeAllowed = properties.UpgradeAllowed;
        ret.RenewUpgradeAllowed = properties.RenewUpgradeAllowed;
        ret.AutoRenewDeviceId = properties.AutoRenewDeviceId;
        ret.RenewMonths = properties.RenewMonths;
        ret.ModelTypeCode = properties.ModelTypeCode;
        ret.OriginalName = properties.OriginalName;
        ret.ServerName = properties.ServerName;
        ret.BundleCode = properties.BundleCode;
        ret.BundleProjectName = properties.BundleProjectName;
        return ret;
    }
    private static ServerComponentResponseDto MapToResponse(this ServerComponent component)
    {
        var ret = new ServerComponentResponseDto();
        ret.Name = component.Name;
        ret.Quantity = component.Quantity;
        return ret;
    }

    private static ServerPleskLicenseResponseDto MapToResponse(this ServerPleskLicense pleskLicense)
    {
        var ret = new ServerPleskLicenseResponseDto();

        ret.ServerIp = pleskLicense.ServerIp;
        ret.ActivationDate = pleskLicense.ActivationDate;
        ret.Description = pleskLicense.Description;
        ret.ActivationCode = pleskLicense.ActivationCode;
        ret.Code = pleskLicense.Code;
        ret.Addons = pleskLicense.Addons?.Select(s => new ServerPleskLicenseResponseAddonDto()
        {
            ActivationDate = s.ActivationDate,
            Code = s.Code,
            Description = s.Description,
        }).ToList() ?? new List<ServerPleskLicenseResponseAddonDto>();
        return ret;
    }
    public static ServerIpAddressListResponseDto? MapToResponse([NotNull] this ServerIpAddressList ipAddresses, HttpRequest httpRequest)
    {
        var ret = new ServerIpAddressListResponseDto()
        {
            TotalCount = ipAddresses.TotalCount,
            Values = ipAddresses.Values.Select(s => s.MapToServerResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static ServerIpAddressResponseDto? MapToServerResponse(this IpAddress ipAddress)
    {
        ipAddress.ThrowIfNull();
        var ret = new ServerIpAddressResponseDto();
        ret.Id = ipAddress.Id;
        ret.Ip = ipAddress.Ip;
        ret.HostNames.AddRange(ipAddress.HostNames ?? new Collection<string>());
        ret.Description = ipAddress.Description;
        ret.Status = ipAddress.Status;
        ret.Type = ipAddress.Type;

        return ret;
    }
    #endregion

    #region ToModel

    public static ServerIpAddressList Map(this LegacyListResponse<LegacyIpAddress> ipAddresses)
    {
        return new ServerIpAddressList
        {
            TotalCount = ipAddresses.TotalItems,
            Values = ipAddresses.Items.Select(s => new IpAddress()
            {
                Id = s.IpAddressId,
                Description = s.CustomName,
                HostNames = s.Hosts ?? new List<string>(),
                Ip = s.IpAddress,
                Status = s.Status.Map(),
                Type = s.IpType.MapIpAddressType()
            }).ToList()
        };
    }

    public static IEnumerable<ServerCatalogItem> MapToServerCatalog(
        this IEnumerable<LegacyCatalogItem> legacyCatalogItem,
        IEnumerable<InternalServerCatalog> serverCatalog,
        string? language)
    {
        legacyCatalogItem.ThrowIfNull();

        var catalogItems = new List<ServerCatalogItem>();

        foreach (var legacyItem in legacyCatalogItem)
        {
            var catalogModel = serverCatalog.FirstOrDefault(c => (c.ProductCode?.Equals(legacyItem.ProductCode, StringComparison.OrdinalIgnoreCase) ?? false));

            var catalogData = catalogModel?.Data.FirstOrDefault(d => d.Language == language)
               ?? catalogModel?.Data.FirstOrDefault(d => d.Language == "it");

            var isOfferCategory = legacyItem.Category == "OFFERTE";

            catalogItems.Add(
                new ServerCatalogItem()
                {
                    IsSoldOut = legacyItem.IsSoldOut,
                    Code = legacyItem.ProductCode,
                    Price = legacyItem.Price,
                    Name = legacyItem.DisplayName,
                    Category = legacyItem.Category,
                    Location = catalogModel?.Location ?? "ITAR-Arezzo",
                    SetupFeePrice = legacyItem.SetupFeePrice,
                    DiscountedPrice = legacyItem.DiscountedPrice,
                    Processor = isOfferCategory
                        ? legacyItem.BaseConfigProducts?.FirstOrDefault(f => f.ProductCode?.EndsWith("-CPU", StringComparison.OrdinalIgnoreCase) ?? false)?.ProductName
                        : catalogData?.Cpu,
                    Connectivity = catalogData?.Connectivity ?? "1 Gb/s",
                    Hdd = isOfferCategory
                        ? legacyItem.BaseConfigProducts?.FirstOrDefault(f => f.ProductCode?.EndsWith("-HDD", StringComparison.OrdinalIgnoreCase) ?? false)?.ProductName
                        : catalogData?.Hdd,
                    Ram = isOfferCategory
                        ? legacyItem.BaseConfigProducts?.FirstOrDefault(f => f.ProductCode?.EndsWith("-RAM", StringComparison.OrdinalIgnoreCase) ?? false)?.ProductName
                        : catalogData?.Ram,
                    ServerName = isOfferCategory
                        ? legacyItem.BaseConfigProducts?.FirstOrDefault(f => f.ProductCode?.EndsWith("-HW", StringComparison.OrdinalIgnoreCase) ?? false)?.ProductName
                        : catalogModel?.ServerName,
                }
            );
        }

        return catalogItems;
    }

    public static Server MapToListItem([NotNull] this LegacyServerListItem server, string userId, [NotNull] Providers.Models.Projects.Project project, Location? location, IEnumerable<InternalServerCatalog> serverCatalog)
    {
        var ret = new Server()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalServer.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Server.Value,
                    Name = Typologies.Server.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture)
                }
            },
            CreatedBy = userId,
            CreationDate = server.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = server.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = server.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = server.ActivationDate.ToDateTimeOffset(),
                State = server.Status == ServerStatuses.Active 
                ||
                server.Status == ServerStatuses.Activating
                ? server.Status.ToString() : StatusValues.Suspended.Value,
            },
            UpdateDate = server.ActivationDate.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Server.CreateUri(project.Metadata.Id!, server.Id),
            Properties = new ServerProperties()
            {
                ActivationDate = server.ActivationDate.ToDateTimeOffset(),
                DueDate = server.ExpirationDate is null ? server.ExpirationDate : server.ExpirationDate.ToDateTimeOffset(),
                Gpu = server.GPU,
                Hdd = server.HDD,
                OperatingSystem = server.OS,
                Model = server.Model,
                Ram = server.RAM,
                Folders = server.IncludedInFolders ?? new List<string>(),
                IpAddress = server.IpAddress,
                Processor = server.CPU,
                ModelTypeCode = server.ModelTypeCode,
                ServerName = serverCatalog.FirstOrDefault(sn => sn.Model?.Equals(server.Model, StringComparison.OrdinalIgnoreCase) ?? false)?.ServerName ?? string.Empty,
            }
        };

        return ret;
    }

    public static Server MapToDetail([NotNull] this LegacyServerDetail server, string userId, [NotNull] Providers.Models.Projects.Project project, Location? location, string serverName, bool isResellerCustomer)
    {
        var ret = new Server()
        {
        Category = new Category()
        {
            Name = Categories.BaremetalServer.Value,
            Provider = "Aruba.Baremetal",
            Typology = new Typology()
            {
                Id = Typologies.Server.Value,
                Name = Typologies.Server.Value.ToUpperInvariant()
            }
        },
        CreatedBy = userId,
        CreationDate = server.ActivationDate.ToDateTimeOffset(),
        Ecommerce = null,
        Id = server.Id.ToString(CultureInfo.InvariantCulture),
        Location = location,
        Name = string.IsNullOrWhiteSpace(server.CustomName) ? server.Name : server.CustomName,
        Project = new Models.Project()
        {
            Id = project.Metadata.Id,
            Name = project.Metadata.Name,
        },
        Status = new Status()
        {
            CreationDate = server.ActivationDate.ToDateTimeOffset(),
            State = server.Status == ServerStatuses.Active
            ||
            server.Status == ServerStatuses.Activating
            ? server.Status.ToString() : StatusValues.Suspended.Value,
        },
        UpdateDate = server.ActivationDate.ToDateTimeOffset(),
        UpdatedBy = userId,
        Version = new Models.Version()
        {
            Data = new DataVersion()
            {
                Current = 1
            }
        },
        Uri = Typologies.Server.CreateUri(project.Metadata.Id!, server.Id),
        Properties = server.MapProperties(serverName, isResellerCustomer)
    };

    return ret;
    }

    private static ServerProperties MapProperties(this LegacyServerDetail server, string serverName, bool isResellerCustomer)
    {
        server.ThrowIfNull();
        return new ServerProperties()
        {
            ActivationDate = server.ActivationDate.ToDateTimeOffset(),
            DueDate = server.ExpirationDate is null ? server.ExpirationDate : server.ExpirationDate.ToDateTimeOffset(),
            Gpu = server.GPU,
            Hdd = server.Hdd,
            OperatingSystem = server.OS,
            Ram = server.RAM,
            Processor = server.CPU,
            IpAddress = server.IpAddress,
            MonthlyUnitPrice = server.MonthlyUnitPrice,
            AutoRenewEnabled = server.AutoRenewEnabled,
            AutoRenewAllowed = server.AutoRenewAllowed,
            AutoRenewDeviceId = server.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewMonths = server.AutoRenewInfo?.RenewMonths ?? 0,
            Folders = server.IncludedInFolders ?? new List<string>(),
            RenewAllowed = server.RenewAllowed,
            RenewUpgradeAllowed = server.RenewUpgradeAllowed,
            OriginalName = server.Name,
            ModelTypeCode = server.ModelTypeCode,
            UpgradeAllowed = server.UpgradeAllowed,
            Model = server.Model,
            Components = server.Components.Map(),
            PleskLicense = server.PleskLicensesInfo.Map(server.IpAddress),
            ServerName = serverName,
            ShowVat = server.AutoRenewEnabled == true
            && server.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false,
            BundleCode = server.BundleCode,
            BundleProjectName = server.BundleProjectName,
        };
    }

    private static ServerPleskLicense? Map(this IEnumerable<LegacyServerPleskLicense> licenses, string? serverIp)
    {
        if (!(licenses?.Any(a => !a.isAddon) ?? false))
        {
            return null;
        }
        var mainLicense = licenses.First(f => !f.isAddon);

        return new ServerPleskLicense()
        {
            ActivationCode = mainLicense.ActivationCode,
            ActivationDate = mainLicense.ActivationDate,
            Code = mainLicense.LicenseCode,
            Description = mainLicense.Description,
            ServerIp = serverIp,
            Addons = licenses.Where(w => w.isAddon).Select(s => new ServerPleskLicenseAddon()
            {
                ActivationDate = s.ActivationDate,
                Code = s.LicenseCode,
                Description = s.Description,
            }).ToList()
        };
    }

    private static IEnumerable<ServerComponent> Map(this IEnumerable<LegacyComponent>? components)
    {
        if (components == null)
        {
            return new List<ServerComponent>();
        }
        return components.Select(s => s.Map()!)
            .Where(w => w != null)
            .ToList();
    }

    private static ServerComponent? Map(this LegacyComponent? component)
    {
        if (component == null)
        {
            return null;
        }

        return new ServerComponent()
        {
            Name = component.Name,
            Quantity = component.Quantity
        };
    }

    public static BaremetalResource Map(this Server server, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = server.Id,
            Uri = server.Uri,
            Name = server.Name,
            Location = new LocationData()
            {
                City = server.Location?.City,
                Name = server.Location?.Name,
                Code = server.Location?.Code,
                Country = server.Location?.Country,
                Value = server.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = server.Project?.Id,
            },
            Tags = server.Tags,
            CreationDate = server.CreationDate,
            UpdateDate = server.UpdateDate,
            CreatedBy = server.CreatedBy,
            UpdatedBy = server.UpdatedBy,
            LinkedResources = server.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = server.Status.State,
                CreationDate = server.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = server.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = server.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = server.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = server.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = server.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = server.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = server.Category?.Typology?.Id,
                    Name = server.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = server.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = server.Version?.Data?.Current ?? default,
                    Previous = server.Version?.Data?.Previous,
                }
            }
        };
    }
    #endregion

}

