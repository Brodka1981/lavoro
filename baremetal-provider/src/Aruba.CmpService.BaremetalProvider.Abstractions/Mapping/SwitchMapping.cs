using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Microsoft.AspNetCore.Http;
using Throw;
using YamlDotNet.Core;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class SwitchMapping
{
    #region Response
    public static SwitchCatalogResponseDto? MapToResponse([NotNull] this SwitchCatalog catalog, HttpRequest httpRequest)
    {
        var ret = new SwitchCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static SwitchCatalogItemResponseDto? MapToResponse(this SwitchCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new SwitchCatalogItemResponseDto();
        ret.IsSoldOut = item.IsSoldOut;
        ret.Code = item.Code;
        ret.Category = item.Category;
        ret.SetupFeePrice = item.SetupFeePrice;
        ret.DiscountedPrice = item.DiscountedPrice;
        ret.Price = item.Price;
        ret.Ports = item.Ports;
        ret.Location = item.Location;
        ret.Name = item.Name;
        return ret;
    }
    public static SwitchListResponseDto? MapToResponse([NotNull] this SwitchList switches, HttpRequest httpRequest)
    {
        var ret = new SwitchListResponseDto()
        {
            TotalCount = switches.TotalCount,
            Values = switches.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }
    public static SwitchResponseDto? MapToResponse(this Switch @switch)
    {
        if (@switch == null)
        {
            return null;
        }
        var ret = new SwitchResponseDto();
        ret.Status = @switch.Status!.MapToResponse();
        ret.Metadata = @switch.MapMetadataToResponse();
        ret.Properties = @switch.Properties!.MapToResponse();
        return ret;
    }

    private static SwitchPropertiesResponseDto? MapToResponse(this SwitchProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new SwitchPropertiesResponseDto()
        {
            ActivationDate = properties.ActivationDate,
            Admin = properties.Admin,
            AutoRenewEnabled = properties.AutoRenewEnabled,
            AutoRenewAllowed = properties.AutoRenewAllowed,
            AutoRenewDeviceId = properties.AutoRenewDeviceId,
            RenewMonths = properties.RenewMonths,
            DueDate = properties.DueDate,
            Folders = properties.Folders ?? new List<string>(),
            MonthlyUnitPrice = properties.MonthlyUnitPrice,
            ShowVat = properties.ShowVat,
            RenewAllowed = properties.RenewAllowed,
            Model = properties.Model
        };
        return ret;
    }
    #endregion

    #region ToModel
    public static IEnumerable<SwitchCatalogItem> MapToSwitchCatalog(this IEnumerable<LegacyCatalogItem> legacyCatalogItems, IEnumerable<InternalSwitchCatalog> switchCatalog)
    {
        legacyCatalogItems.ThrowIfNull();
        var switchItems = new List<SwitchCatalogItem>();

        foreach (var legacyItem in legacyCatalogItems)
        {
            var catalogData = switchCatalog.FirstOrDefault(s => s.Code == legacyItem.ProductCode);
            switchItems.Add(
                new SwitchCatalogItem
                {
                    IsSoldOut = legacyItem.IsSoldOut,
                    Code = legacyItem.ProductCode,
                    Price = legacyItem.Price,
                    Name = legacyItem.DisplayName,
                    Category = legacyItem.Category,
                    Location = catalogData?.Location,
                    SetupFeePrice = legacyItem.SetupFeePrice,
                    DiscountedPrice = legacyItem.DiscountedPrice,
                    Ports = catalogData?.Ports,
                });
        }

        return switchItems;
    }

    public static Switch MapToListitem(this LegacySwitchListItem @switch, string userId, Providers.Models.Projects.Project project, Location? location)
    {
        @switch.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Switch()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Switch.Value,
                    Name = Typologies.Switch.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = @switch.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = @switch.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = @switch.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = @switch.ActivationDate.ToDateTimeOffset(),
                State = @switch.Status.ToString(),
            },
            UpdateDate = @switch.ActivationDate.ToDateTimeOffset(),
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Switch.CreateUri(project.Metadata.Id!, @switch.Id),
            Properties = @switch.MapListitemProperties()
        };

        return ret;
    }
    private static SwitchProperties MapListitemProperties(this LegacySwitchListItem @switch)
    {
        @switch.ThrowIfNull();
        return new SwitchProperties()
        {
            ActivationDate = @switch.ActivationDate.ToDateTimeOffset(),
            DueDate = @switch.ExpirationDate,
            Admin = @switch.Name,
            Model = @switch.Model,
            Folders = @switch.IncludedInFolders ?? new List<string>()
        };
    }
    public static Switch MapToDetail(this LegacySwitchDetail @switch, string userId, Providers.Models.Projects.Project project, Location? location, bool isResellerCustomer)
    {
        @switch.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new Switch()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalNetwork.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.Switch.Value,
                    Name = Typologies.Switch.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = @switch.ActivationDate,
            Ecommerce = null,
            Id = @switch.Id.ToString(CultureInfo.InvariantCulture),
            Location = location,
            Name = string.IsNullOrWhiteSpace(@switch.CustomName) ? @switch.Name : @switch.CustomName,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = @switch.ActivationDate,
                State = @switch.Status.ToString()
            },
            UpdateDate = @switch.ActivationDate,
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.Switch.CreateUri(project.Metadata.Id!, @switch.Id),
            Properties = @switch.MapProperties(isResellerCustomer)
        };

        return ret;
    }
    private static SwitchProperties MapProperties(this LegacySwitchDetail @switch, bool isResellerCustomer)
    {
        @switch.ThrowIfNull();
        return new SwitchProperties()
        {
            ActivationDate = @switch.ActivationDate.ToDateTimeOffset(),
            DueDate = @switch.ExpirationDate,
            Admin = @switch.Name,
            Model = @switch.Model,
            AutoRenewEnabled = @switch.AutoRenewEnabled,
            AutoRenewAllowed = @switch.AutoRenewAllowed,
            AutoRenewDeviceId = @switch.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewMonths = @switch.AutoRenewInfo?.RenewMonths,
            Folders = @switch.IncludedInFolders ?? new List<string>(),
            MonthlyUnitPrice = @switch.MonthlyUnitPrice,
            RenewAllowed = @switch.RenewAllowed,
            ShowVat = @switch.AutoRenewEnabled == true
            && @switch.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false
        };
    }

    public static BaremetalResource Map(this Switch @switch, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = @switch.Id,
            Uri = @switch.Uri,
            Name = @switch.Name,
            Location = new LocationData()
            {
                City = @switch.Location?.City,
                Name = @switch.Location?.Name,
                Code = @switch.Location?.Code,
                Country = @switch.Location?.Country,
                Value = @switch.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = @switch.Project.Id,
            },
            Tags = @switch.Tags,
            CreationDate = @switch.CreationDate,
            UpdateDate = @switch.UpdateDate,
            CreatedBy = @switch.CreatedBy,
            UpdatedBy = @switch.UpdatedBy,
            LinkedResources = @switch.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = @switch.Status.State,
                CreationDate = @switch.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = @switch.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = @switch.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = @switch.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = @switch.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = @switch.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = @switch.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = @switch.Category?.Typology?.Id,
                    Name = @switch.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = @switch.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = @switch.Version?.Data?.Current ?? default,
                    Previous = @switch.Version?.Data?.Previous,
                }
            }
        };
    }
    #endregion
}

