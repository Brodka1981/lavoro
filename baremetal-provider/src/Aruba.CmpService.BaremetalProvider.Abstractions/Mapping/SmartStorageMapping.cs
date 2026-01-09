using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Microsoft.AspNetCore.Http;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class SmartStorageMapping
{
    #region Response

    public static IEnumerable<SmartStorageFoldersItemResponseDto?> MapToResponse([NotNull] this IEnumerable<SmartStorageFoldersItem> folders, HttpRequest httpRequest)
    {
        return folders.Select(s => s.MapToResponse()).ToList();
    }

    private static SmartStorageFoldersItemResponseDto? MapToResponse(this SmartStorageFoldersItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new SmartStorageFoldersItemResponseDto();
        ret.Name = item.Name;
        ret.SmartFolderID = item.SmartFolderID;
        ret.PositionDisplay = item.PositionDisplay;
        ret.UsedSpace = item.UsedSpace;
        ret.AvailableSpace = item.AvailableSpace;
        ret.Readonly = item.Readonly;
        ret.IsRootFolder = item.IsRootFolder;
        return ret;
    }

    public static SmartStorageCatalogResponseDto? MapToResponse([NotNull] this SmartStorageCatalog catalog, HttpRequest httpRequest)
    {
        var ret = new SmartStorageCatalogResponseDto()
        {
            TotalCount = catalog.TotalCount,
            Values = catalog.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    private static SmartStorageCatalogItemResponseDto? MapToResponse(this SmartStorageCatalogItem item)
    {
        if (item == null)
        {
            return null;
        }
        var ret = new SmartStorageCatalogItemResponseDto();
        ret.IsSoldOut = item.IsSoldOut;
        ret.Code = item.Code;
        ret.Name = item.Name;
        ret.Category = item.Category;
        ret.Snapshot = item.Snapshot;
        ret.Replica = item.Replica;
        ret.Price = item.Price;
        ret.Size = item.Size;

        return ret;
    }

    public static SmartStorageListResponseDto? MapToResponse([NotNull] this SmartStorageList smartStorages, HttpRequest httpRequest)
    {
        var ret = new SmartStorageListResponseDto()
        {
            TotalCount = smartStorages.TotalCount,
            Values = smartStorages.Values.Select(s => s.MapToResponse()!).ToList()
        };
        ret.FillPaginationData(httpRequest);
        return ret;
    }

    public static SmartStorageResponseDto? MapToResponse(this SmartStorage smartStorage)
    {
        if (smartStorage == null)
        {
            return null;
        }
        var ret = new SmartStorageResponseDto();
        ret.Status = smartStorage.Status!.MapToResponse();
        ret.Metadata = smartStorage.MapMetadataToResponse();
        ret.Properties = smartStorage.Properties!.MapToResponse();
        return ret;
    }

    public static SmartStorageSnapshotsResponseDto? MapToResponse(this SmartStorageSnapshots snapshots)
    {
        if (snapshots == null)
        {
            return null;
        }
        var ret = new SmartStorageSnapshotsResponseDto();
        ret.UsedSnapshots = snapshots.UsedSnapshots;
        ret.AvailableSnapshots = snapshots.AvailableSnapshots;
        ret.TotalSnapshots = snapshots.TotalSnapshots;
        ret.Snapshots = snapshots.Snapshots.MapManualSnapshots();
        ret.SnapshotTasks = snapshots.SnapshotTasks.MapSnapshotTasks();
        return ret;
    }

    public static IEnumerable<SnapshotDto> MapManualSnapshots(this IEnumerable<Snapshot> legacySnapshots)
    {
        if (legacySnapshots is null)
        {
            return new List<SnapshotDto>();
        }
        return legacySnapshots.Select(s => new SnapshotDto
        {
            CreationDate = s.CreationDate,
            ManualSnapshot = true,
            Name = s.Name,
            RawReferencedSize = s.RawReferencedSize,
            RawSize = s.RawSize,
            ReferencedSize = s.ReferencedSize,
            Size = s.Size,
            SmartFolderName = s.SmartFolderName,
            SnapshotId = s.SnapshotId
        }).ToList();
    }

    public static IEnumerable<SnapshotTasksDto> MapSnapshotTasks(this IEnumerable<SnapshotTask> legacySnapshots)
    {
        if (legacySnapshots is null)
        {
            return new List<SnapshotTasksDto>();
        }
        return legacySnapshots.Select(s => new SnapshotTasksDto
        {
            CreationDate = s.CreationDate,
            Enabled = s.Enabled,
            Iterations = s.Iterations,
            Schedule = s.Schedule!.MapSchedule(),
            SmartFolderName = s.SmartFolderName,
            ScheduleType = s.ScheduleType,
            SnapshotTaskId = s.SnapshotTaskId
        }).ToList();
    }

    public static ScheduleDto MapSchedule(this Schedule legacySchedule)
    {
        if (legacySchedule is null)
        {
            return new ScheduleDto();
        }

        return new ScheduleDto
        {
            DayOfMonth = legacySchedule.DayOfMonth,
            DayOfWeek = legacySchedule.DayOfWeek,
            Hours = legacySchedule.Hours,
            Minutes = legacySchedule.Minutes,
            Month = legacySchedule.Month
        };
    }

    private static SmartStoragePropertiesResponseDto? MapToResponse(this SmartStorageProperties properties)
    {
        properties.ThrowIfNull();
        var ret = new SmartStoragePropertiesResponseDto()
        {
            ActivationDate = properties.ActivationDate,
            DueDate = properties.DueDate,
            Folders = properties.Folders,
            Replica = properties.Replica,
            Samba = properties.Samba,
            Package = properties.Package,
            Server = properties.Server,
            Username = properties.Username,
            MonthlyUnitPrice = properties.MonthlyUnitPrice,
            ShowVat = properties.ShowVat,
            AutoRenewEnabled = properties.AutoRenewEnabled,
            RenewAllowed = properties.RenewAllowed,
            UpgradeAllowed = properties.UpgradeAllowed,
            AutoRenewDeviceId = properties.AutoRenewDeviceId,
            RenewMonths = properties.RenewMonths,
            IsFirstSetupDone = properties.IsFirstSetupDone,
        };
        return ret;
    }
    #endregion

    #region ToModel
    public static IEnumerable<SmartStorageCatalogItem> MapToSmartStorageCatalog(this IEnumerable<LegacyCatalogItem> legacyCatalogItems, IEnumerable<InternalSmartStorageCatalog> smartStorageCatalog)
    {
        legacyCatalogItems.ThrowIfNull();

        var result = new List<SmartStorageCatalogItem>();

        foreach (var legacyItem in legacyCatalogItems)
        {
            var internalCatalog = smartStorageCatalog.FirstOrDefault(x => x.Code == legacyItem.ProductCode);

            result.Add(new SmartStorageCatalogItem()
            {
                Name = legacyItem.DisplayName,
                Price = legacyItem.Price,
                Replica = internalCatalog?.Replica ?? false,
                Size = internalCatalog?.Size,
                Snapshot = internalCatalog?.Snapshot,
                IsSoldOut = legacyItem.IsSoldOut,
                Category = legacyItem.Category,
                Code = legacyItem.ProductCode,
            });
        }

        return result;
    }

    public static IEnumerable<SmartStorageFoldersItem> MapToSmartStorageFolders(this IEnumerable<LegacySmartFoldersItem> smartFoldersItem)
    {
        smartFoldersItem.ThrowIfNull();
        return smartFoldersItem.Select(s => new SmartStorageFoldersItem()
        {
            AvailableSpace = s.AvailableSpace,
            SmartFolderID = s.SmartFolderID,
            IsRootFolder = s.IsRootFolder,
            Name = s.Name,
            PositionDisplay = s.PositionDisplay,
            Readonly = s.Readonly,
            UsedSpace = s.UsedSpace
        }).ToList();
    }

    public static SmartStorage MapToListitem(this LegacySmartStorageListItem smartStorage, string userId, Providers.Models.Projects.Project project)
    {
        smartStorage.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new SmartStorage()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalSmartStorage.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.SmartStorage.Value,
                    Name = Typologies.SmartStorage.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = smartStorage.ActivationDate.ToDateTimeOffset(),
            Ecommerce = null,
            Id = smartStorage.Id.ToString(CultureInfo.InvariantCulture),
            Name = smartStorage.Name,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = smartStorage.ActivationDate,
                State = smartStorage.Status == SmartStoragesStatuses.Suspended 
                ||
                smartStorage.Status == SmartStoragesStatuses.Activating
                ? smartStorage.Status.ToString() : StatusValues.Active.Value,
            },
            UpdateDate = smartStorage.ActivationDate,
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.SmartStorage.CreateUri(project.Metadata.Id!, smartStorage.Id),
            Properties = smartStorage.MapListitemProperties()
        };

        return ret;
    }

    private static SmartStorageProperties MapListitemProperties(this LegacySmartStorageListItem smartStorage)
    {
        smartStorage.ThrowIfNull();
        return new SmartStorageProperties()
        {
            ActivationDate = smartStorage.ActivationDate.ToDateTimeOffset(),
            DueDate = smartStorage.ExpirationDate,
            Package = smartStorage.Model,
            Folders = smartStorage.IncludedInFolders ?? new List<string>(),
        };
    }

    public static SmartStorage MapToDetail(this LegacySmartStorageDetail smartStorage, string userId, Providers.Models.Projects.Project project, bool isResellerCustomer)
    {
        smartStorage.ThrowIfNull();
        project.ThrowIfNull();
        var ret = new SmartStorage()
        {
            Category = new Category()
            {
                Name = Categories.BaremetalSmartStorage.Value,
                Provider = "Aruba.Baremetal",
                Typology = new Typology()
                {
                    Id = Typologies.SmartStorage.Value,
                    Name = Typologies.SmartStorage.Value.ToUpperInvariant()
                }
            },
            CreatedBy = userId,
            CreationDate = smartStorage.ActivationDate,
            Ecommerce = null,
            Id = smartStorage.Id.ToString(CultureInfo.InvariantCulture),
            Name = string.IsNullOrWhiteSpace(smartStorage.CustomName) ? smartStorage.Name : smartStorage.CustomName,
            Project = new Models.Project()
            {
                Id = project.Metadata.Id,
                Name = project.Metadata.Name,
            },
            Status = new Status()
            {
                CreationDate = smartStorage.ActivationDate,
                State = (smartStorage?.FirstSetupDone ?? false) ? smartStorage.SmartStorageInfo.Status.MapStatus()?.Value : StatusValues.ToBeActivated.Value
            },
            UpdateDate = smartStorage.ActivationDate,
            UpdatedBy = userId,
            Version = new Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 1
                }
            },
            Uri = Typologies.SmartStorage.CreateUri(project.Metadata.Id!, smartStorage.Id),
            Properties = smartStorage.MapDetailProperties(isResellerCustomer)
        };

        return ret;
    }

    private static StatusValues? MapStatus(this SmartStorageInfoStatus? status)
    {
        switch (status)
        {
            case SmartStorageInfoStatus.Active:
                return StatusValues.Active;
            case SmartStorageInfoStatus.Creating:
                return StatusValues.Activating;
            case SmartStorageInfoStatus.Deleted:
                return StatusValues.Deleted;
            case SmartStorageInfoStatus.Maintenance:
                return StatusValues.InMaintenance;
            case SmartStorageInfoStatus.Suspend:
                return StatusValues.Suspended;
            case SmartStorageInfoStatus.Unavailable:
                return StatusValues.Unavailable;
            case SmartStorageInfoStatus.Upgrading:
                return StatusValues.Updating;
            default:
                return null;
        }
    }

    private static SmartStorageProperties MapDetailProperties(this LegacySmartStorageDetail smartStorage, bool isResellerCustomer)
    {
        smartStorage.ThrowIfNull();
        return new SmartStorageProperties()
        {
            DueDate = smartStorage.ExpirationDate,
            ActivationDate = smartStorage.ActivationDate,
            Folders = smartStorage.IncludedInFolders ?? new List<string>(),
            Package = smartStorage.Model,
            Replica = smartStorage.HasReplica,
            Samba = smartStorage.SambaAddress,
            Server = smartStorage.ServerAddress,
            Username = smartStorage.Username,
            MonthlyUnitPrice = smartStorage.MonthlyUnitPrice,
            UpgradeAllowed = smartStorage.UpgradeAllowed,
            RenewAllowed = smartStorage.RenewAllowed,
            AutoRenewEnabled = smartStorage.AutoRenewEnabled,
            AutoRenewDeviceId = smartStorage.AutoRenewInfo?.CreateCmpDeviceId(),
            RenewMonths = smartStorage.AutoRenewInfo?.RenewMonths ?? 0,
            IsFirstSetupDone = smartStorage.FirstSetupDone,
            ShowVat = smartStorage.AutoRenewEnabled == true
            && smartStorage.AutoRenewInfo?.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false
        };
    }

    public static IEnumerable<SmartStorageProtocol> MapProtocolList(this IEnumerable<LegacyProtocol> legacyProtocols)
    {
        return legacyProtocols.Select(p => p.MapToModel()).ToList();
    }

    private static SmartStorageProtocol MapToModel(this LegacyProtocol legacyProtocol)
    {
        return new SmartStorageProtocol()
        {
            Error = legacyProtocol.Error,
            ErrorMessage = legacyProtocol.ErrorMessage,
            ServiceStatus = (ServiceStatus)legacyProtocol.ServiceStatus,
            ServiceType = (ServiceType)legacyProtocol.ServiceType,
        };
    }

    public static SmartStorageStatistics MapStatistics(this LegacyStatistics legacyStatistics)
    {
        if (legacyStatistics is null)
            return new SmartStorageStatistics();

        return new SmartStorageStatistics
        {
            SmartFolders = legacyStatistics.SmartFolders
                .Select(s => new StatisticsSmartFolder
                {
                    Name = s.Name,
                    Size = new StatisticsSize(s.Size, s.RawSize)
                }).ToList(),
            Snapshots = legacyStatistics.Snapshots
                .Select(s => new StatisticsSnapshot
                {
                    Name = s.Name,
                    Size = SmartFolderTotalSize(s.SmartFolderName!, legacyStatistics.Snapshots.ToList()),
                    ReferencedSize = new StatisticsSize(s.ReferencedSize, s.RawReferencedSize),
                    SmartFolderName = s.SmartFolderName,
                }).ToList(),
            TotalSmartFolders = legacyStatistics.TotalSmartFolders,
            TotalSmartFoldersSize = new StatisticsSize(legacyStatistics.TotalSmartFoldersSize, legacyStatistics.RawTotalSmartFoldersSize),
            TotalSnapshots = legacyStatistics.TotalSnapshots,
            TotalSnapshotsSize = new StatisticsSize(legacyStatistics.TotalSnapshotsSize, legacyStatistics.RawTotalSnapshotsSize),
            TotalDiskSpace = new StatisticsSize(legacyStatistics.TotalDiskSpace, legacyStatistics.RawTotalDiskSpace),
            AvailableDiskSpace = new StatisticsSize(legacyStatistics.AvailableDiskSpace, legacyStatistics.RawAvailableDiskSpace),
            ReservedDiskSpace = new StatisticsSize(legacyStatistics.ReservedDiskSpace, legacyStatistics.RawReservedDiskSpace),
            TotalUsedSpace = new StatisticsSize(legacyStatistics.TotalUsedSpace, legacyStatistics.RawTotalUsedSpace)
        };
    }

    private static StatisticsSize SmartFolderTotalSize(string smartFolderName,List<LegacySnapshot> legacySnapshots)
    {
        var statisticSize = new StatisticsSize("0",0);
        statisticSize.RawSize = legacySnapshots.Where(s => s.SmartFolderName!.Equals(smartFolderName,StringComparison.OrdinalIgnoreCase))
        .Sum(s => s.RawReferencedSize);
        statisticSize.Size = ConvertDiskSizeToString(statisticSize.RawSize);
        return statisticSize;
    }
    private static string ConvertDiskSizeToString(long diskSize)
    {
        var result = string.Empty;
        int exponent = 1;
        string suffix = "KB";


        if (diskSize >= Math.Pow(1024, 4))
        {
            exponent = 4;
            suffix = "TB";
        }
        else if (diskSize >= Math.Pow(1024, 3))
        {
            exponent = 3;
            suffix = "GB";
        }
        else if (diskSize >= Math.Pow(1024, 2))
        {
            exponent = 2;
            suffix = "MB";
        }

        double value = diskSize / (Math.Pow(1024, exponent));
        result = $"{value.ToString("N2", CultureInfo.InvariantCulture)}{suffix}";
        return result;
    }


    public static SmartStorageSnapshots MapSnapshots(this LegacySnapshots legacySnapshots)
    {
        if (legacySnapshots is null)
        {
            return new SmartStorageSnapshots { AvailableSnapshots=0,TotalSnapshots=0,UsedSnapshots=0};
        }


        return new SmartStorageSnapshots
        {
            UsedSnapshots = legacySnapshots.UsedSnapshots,
            TotalSnapshots = legacySnapshots.TotalSnapshots,
            AvailableSnapshots = legacySnapshots.AvailableSnapshots,
            Snapshots = legacySnapshots.Snapshots.MapManualSnapshots(),
            SnapshotTasks = legacySnapshots.SnapshotTasks.MapSnapshotTasks(),
        };
    }

    public static IEnumerable<Snapshot> MapManualSnapshots(this IEnumerable<LegacyManualSnapshots> legacySnapshots)
    {
        if (legacySnapshots is null)
        {
            return new List<Snapshot>();
        }
        return legacySnapshots.Select(s => new Snapshot
        {
            CreationDate = s.CreationDate,
            ManualSnapshot = true,
            Name = s.Name,
            RawReferencedSize = s.RawReferencedSize,
            RawSize = s.RawSize,
            ReferencedSize = s.ReferencedSize,
            Size = s.Size,
            SmartFolderName = s.SmartFolderName,
            SnapshotId = s.SnapshotId
        }).ToList();
    }

    public static IEnumerable<SnapshotTask> MapSnapshotTasks(this IEnumerable<LegacySnapshotsTasks> legacySnapshots)
    {
        if (legacySnapshots is null)
        {
            return new List<SnapshotTask>();
        }
        return legacySnapshots.Select(s => new SnapshotTask
        {
            CreationDate = s.CreationDate,
            Enabled = s.Enabled,
            Iterations = s.Iterations,
            Schedule = s.Schedule!.MapSchedule(),
            SmartFolderName = s.SmartFolderName,
            ScheduleType = s.ScheduleType,
            SnapshotTaskId = s.SnapshotTaskId
        }).ToList();
    }

    public static Schedule MapSchedule(this LegacySchedule legacySchedule)
    {
        if (legacySchedule is null)
        {
            return new Schedule();
        }

        return new Schedule
        {
            DayOfMonth = legacySchedule.DayOfMonth,
            DayOfWeek = legacySchedule.DayOfWeek,
            Hours = legacySchedule.Hours,
            Minutes = legacySchedule.Minutes,
            Month = legacySchedule.Month
        };
    }

    public static BaremetalResource Map(this SmartStorage smartStorage, AutorenewFolderAction? action)
    {
        return new BaremetalResource
        {
            Action = action,
            Id = smartStorage.Id,
            Uri = smartStorage.Uri,
            Name = smartStorage.Name,
            Location = new LocationData()
            {
                City = smartStorage.Location?.City,
                Name = smartStorage.Location?.Name,
                Code = smartStorage.Location?.Code,
                Country = smartStorage.Location?.Country,
                Value = smartStorage.Location?.Value,
            },
            Project = new ProjectData()
            {
                Id = smartStorage.Project?.Id,
            },
            Tags = smartStorage.Tags,
            CreationDate = smartStorage.CreationDate,
            UpdateDate = smartStorage.UpdateDate,
            CreatedBy = smartStorage.CreatedBy,
            UpdatedBy = smartStorage.UpdatedBy,
            LinkedResources = smartStorage.LinkedResources?.Select(r => new LinkedResourceData()
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
                State = smartStorage.Status.State,
                CreationDate = smartStorage.Status.CreationDate,
                DisableStatusInfo = new DisableStatusInfoData()
                {
                    ReasonDetails = smartStorage.Status.DisableStatusInfo?.Reasons.Select(r => new DisableReasonDetailData()
                    {
                        CreationDate = r.CreationDate,
                        Mode = r.Mode == DisableMode.Automatic ? ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic : ResourceProvider.Common.Messages.v1.Enums.DisableMode.Manual,
                        Note = r.Note,
                        Reason = r.Reason
                    }).ToCollection() as Collection<DisableReasonDetailData> ?? new Collection<DisableReasonDetailData>(),
                    Reasons = smartStorage.Status.DisableStatusInfo?.Reasons.Select(r => r.Reason).ToCollection() as Collection<string> ?? new Collection<string>(),
                    IsDisabled = smartStorage.Status.DisableStatusInfo?.IsDisabled ?? false,
                    PreviousStatus = new PreviousStatusData()
                    {
                        CreationDate = smartStorage.Status.DisableStatusInfo?.PreviousStatus?.CreationDate,
                        State = smartStorage.Status.DisableStatusInfo?.PreviousStatus?.State
                    },
                },
            },
            Category = new CategoryData()
            {
                Name = smartStorage.Category?.Name,
                Typology = new TypologyData()
                {
                    Id = smartStorage.Category?.Typology?.Id,
                    Name = smartStorage.Category?.Typology?.Name,
                }
            },
            Version = new VersionData()
            {
                Model = smartStorage.Version?.Model,
                Data = new DataVersionData()
                {
                    Current = smartStorage.Version?.Data?.Current?? default,
                    Previous = smartStorage.Version?.Data?.Previous,
                }
            }
        };
    }
    #endregion
}
