using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;

public class SmartStorageMappingTests : TestBase
{
    public SmartStorageMappingTests(ITestOutputHelper output)
        : base(output)
    { }

    #region Response
    [Fact]
    [Unit]
    public void SmartStorageCatalog_Success_EmptyItems()
    {
        var catalog = new SmartStorageCatalog();
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);

        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(0);
        mapped.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_Success_WithItems()
    {
        var catalog = new SmartStorageCatalog()
        {
            TotalCount = 1,
            Values = new List<SmartStorageCatalogItem>()
            {
                new SmartStorageCatalogItem()
                {
                    Category = "Category",
                    Name = "Name",
                    Code = "Code",
                    IsSoldOut = true,
                    Price = 2,
                    Replica = true,
                    Size ="10GB",
                    Snapshot = "2"
                }
            }
        };
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);


        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(1);
        mapped.Values.Should().HaveCount(1);
        mapped.Values[0].Category.Should().Be(catalog.Values.First().Category);
        mapped.Values[0].Name.Should().Be(catalog.Values.First().Name);
        mapped.Values[0].Code.Should().Be(catalog.Values.First().Code);
        mapped.Values[0].Replica.Should().Be(catalog.Values.First().Replica);
        mapped.Values[0].IsSoldOut.Should().Be(catalog.Values.First().IsSoldOut);
        mapped.Values[0].Size.Should().Be(catalog.Values.First().Size);
        mapped.Values[0].Price.Should().Be(catalog.Values.First().Price);
        mapped.Values[0].Snapshot.Should().Be(catalog.Values.First().Snapshot);
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_Success_With1ItemsNULL()
    {
        var catalog = new SmartStorageCatalog()
        {
            TotalCount = 1,
            Values = new List<SmartStorageCatalogItem>()
            {
                null
            }
        };
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);


        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(1);
        mapped.Values.Should().HaveCount(1);
        mapped.Values[0].Should().BeNull();
    }

    [Fact]
    [Unit]
    public void SmartStorageListResponse_To_SmartStorageListResponseDto()
    {
        var smartStorageListResponse = new SmartStorageList()
        {
            TotalCount = 1,
            Values = new List<SmartStorage>()
            {
                SmartStorageModel
            }
        };

        var mapped = smartStorageListResponse.MapToResponse(Substitute.For<HttpRequest>());
        mapped.Should().NotBeNull();
        CommonMappingTests.ValidateBaseResource<SmartStorage, SmartStorageResponseDto, SmartStoragePropertiesResponseDto>(smartStorageListResponse.Values.First(), mapped.Values.First(), "aru-25085");
    }

    [Fact]
    [Unit]
    public void SmartStorage_Success()
    {
        var mapped = SmartStorageModel.MapToResponse();
        mapped.Should().NotBeNull();
        CommonMappingTests.ValidateBaseResource<SmartStorage, SmartStorageResponseDto, SmartStoragePropertiesResponseDto>(SmartStorageModel, mapped, "aru-25085");
    }

    [Fact]
    [Unit]
    public void SmartStorage_Null_Success()
    {
        var mapped = ((SmartStorage?)default).MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void SmartStorageFoldersItem_Success_EmptyItems()
    {
        var smartStorageFoldersItems = new List<SmartStorageFoldersItem>();
        var httpContext = new DefaultHttpContext();
        var mapped = smartStorageFoldersItems.MapToResponse(httpContext.Request);

        mapped.Should().NotBeNull();
        mapped.Count().Should().Be(0);
    }

    [Fact]
    [Unit]
    public void SmartStorageFoldersItem_Success_NullItems()
    {
        var smartStorageFoldersItems = new List<SmartStorageFoldersItem>() { null };
        var httpContext = new DefaultHttpContext();
        var mapped = smartStorageFoldersItems.MapToResponse(httpContext.Request);

        mapped.Should().NotBeNull();
        mapped.Count().Should().Be(1);
        mapped.ElementAt(0).Should().BeNull();
    }

    [Fact]
    [Unit]
    public void SmartStorageFoldersItem_Success()
    {
        var smartStorageFoldersItems = new List<SmartStorageFoldersItem>
        {
            new SmartStorageFoldersItem()
            {
                 Name = "a2",
                 AvailableSpace = "1GB",
                 IsRootFolder = true,
                 PositionDisplay = "1",
                 Readonly = false,
                 SmartFolderID = "id1",
                 UsedSpace = "0"
            }
        };
        var httpContext = new DefaultHttpContext();
        var mapped = smartStorageFoldersItems.MapToResponse(httpContext.Request);

        mapped.Should().NotBeNull();
        mapped.Count().Should().Be(1);
        mapped.First().Name.Should().Be(smartStorageFoldersItems[0].Name);
        mapped.First().AvailableSpace.Should().Be(smartStorageFoldersItems[0].AvailableSpace);
        mapped.First().IsRootFolder.Should().Be(smartStorageFoldersItems[0].IsRootFolder);
        mapped.First().PositionDisplay.Should().Be(smartStorageFoldersItems[0].PositionDisplay);
        mapped.First().SmartFolderID.Should().Be(smartStorageFoldersItems[0].SmartFolderID);
        mapped.First().UsedSpace.Should().Be(smartStorageFoldersItems[0].UsedSpace);
    }



    [Fact]
    [Unit]
    public void SmartStorageSnapshotsResponseDto_Success()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = new SmartStorageSnapshots()
        {
            AvailableSnapshots = 1,
            UsedSnapshots = 2,
            TotalSnapshots = 3,
            SnapshotTasks = new List<SnapshotTask>()
            {
                new SnapshotTask()
                {
                    CreationDate = now,
                    Enabled = true,
                    Iterations = 1,
                    Schedule = new Schedule()
                    {
                        DayOfMonth = 1,
                        DayOfWeek = 2,
                        Hours = 3,
                        Minutes = 4,
                        Month = 5
                    }
                }
            },
            Snapshots = new List<Snapshot>()
            {
                new ()
                {
                    CreationDate = DateTime.Now,
                    ManualSnapshot = true,
                    Name = "Name",
                    RawReferencedSize = 1000,
                    RawSize = 2000,
                    ReferencedSize ="1000",
                    Size = "2000",
                    SmartFolderName = "SmartFolderName",
                    SnapshotId = "SnapshotId"
                }
            }
        };

        var httpContext = new DefaultHttpContext();
        var mapped = snapshot.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.UsedSnapshots.Should().Be(snapshot.UsedSnapshots);
        mapped.AvailableSnapshots.Should().Be(snapshot.AvailableSnapshots);
        mapped.TotalSnapshots.Should().Be(snapshot.TotalSnapshots);
        mapped.SnapshotTasks.Should().HaveCount(1);
        mapped.SnapshotTasks.First().Schedule.Should().NotBeNull();
        mapped.SnapshotTasks.First().Schedule?.DayOfMonth.Should().Be(snapshot.SnapshotTasks.First().Schedule?.DayOfMonth);
        mapped.SnapshotTasks.First().Schedule?.DayOfWeek.Should().Be(snapshot.SnapshotTasks.First().Schedule?.DayOfWeek);
        mapped.SnapshotTasks.First().Schedule?.Hours.Should().Be(snapshot.SnapshotTasks.First().Schedule?.Hours);
        mapped.SnapshotTasks.First().Schedule?.Minutes.Should().Be(snapshot.SnapshotTasks.First().Schedule?.Minutes);
        mapped.SnapshotTasks.First().Schedule?.Month.Should().Be(snapshot.SnapshotTasks.First().Schedule?.Month);
        mapped.SnapshotTasks.First().CreationDate.Should().Be(snapshot.SnapshotTasks.First().CreationDate);
        mapped.SnapshotTasks.First().Enabled.Should().Be(snapshot.SnapshotTasks.First().Enabled);
        mapped.SnapshotTasks.First().Iterations.Should().Be(snapshot.SnapshotTasks.First().Iterations);
        mapped.Snapshots.Should().HaveCount(1);
        mapped.Snapshots.First().CreationDate.Should().Be(snapshot.Snapshots.First().CreationDate);
        mapped.Snapshots.First().ManualSnapshot.Should().Be(snapshot.Snapshots.First().ManualSnapshot);
        mapped.Snapshots.First().Name.Should().Be(snapshot.Snapshots.First().Name);
        mapped.Snapshots.First().RawReferencedSize.Should().Be(snapshot.Snapshots.First().RawReferencedSize);
        mapped.Snapshots.First().RawSize.Should().Be(snapshot.Snapshots.First().RawSize);
        mapped.Snapshots.First().ReferencedSize.Should().Be(snapshot.Snapshots.First().ReferencedSize);
        mapped.Snapshots.First().Size.Should().Be(snapshot.Snapshots.First().Size);
        mapped.Snapshots.First().SmartFolderName.Should().Be(snapshot.Snapshots.First().SmartFolderName);
        mapped.Snapshots.First().SnapshotId.Should().Be(snapshot.Snapshots.First().SnapshotId);
    }





    [Fact]
    [Unit]
    public void SmartStorageSnapshotsResponseDto_Success_ScheduleNull()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = new SmartStorageSnapshots()
        {
            AvailableSnapshots = 1,
            UsedSnapshots = 2,
            TotalSnapshots = 3,
            SnapshotTasks = new List<SnapshotTask>()
            {
                new SnapshotTask()
                {
                    CreationDate = now,
                    Enabled = true,
                    Iterations = 1,
                    Schedule = null
                }
            },
            Snapshots = new List<Snapshot>()
            {
                new ()
                {
                    CreationDate = DateTime.Now,
                    ManualSnapshot = true,
                    Name = "Name",
                    RawReferencedSize = 1000,
                    RawSize = 2000,
                    ReferencedSize ="1000",
                    Size = "2000",
                    SmartFolderName = "SmartFolderName",
                    SnapshotId = "SnapshotId"
                }
            }
        };

        var mapped = snapshot.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.UsedSnapshots.Should().Be(snapshot.UsedSnapshots);
        mapped.AvailableSnapshots.Should().Be(snapshot.AvailableSnapshots);
        mapped.TotalSnapshots.Should().Be(snapshot.TotalSnapshots);
        mapped.SnapshotTasks.Should().HaveCount(1);
        mapped.SnapshotTasks.First().Schedule.Should().NotBeNull();
        mapped.SnapshotTasks.First().Schedule?.DayOfMonth.Should().BeNull();
        mapped.SnapshotTasks.First().Schedule?.DayOfWeek.Should().BeNull();
        mapped.SnapshotTasks.First().Schedule?.Hours.Should().BeNull();
        mapped.SnapshotTasks.First().Schedule?.Minutes.Should().BeNull();
        mapped.SnapshotTasks.First().Schedule?.Month.Should().BeNull();
        mapped.SnapshotTasks.First().CreationDate.Should().Be(snapshot.SnapshotTasks.First().CreationDate);
        mapped.SnapshotTasks.First().Enabled.Should().Be(snapshot.SnapshotTasks.First().Enabled);
        mapped.SnapshotTasks.First().Iterations.Should().Be(snapshot.SnapshotTasks.First().Iterations);
        mapped.Snapshots.Should().HaveCount(1);
        mapped.Snapshots.First().CreationDate.Should().Be(snapshot.Snapshots.First().CreationDate);
        mapped.Snapshots.First().ManualSnapshot.Should().Be(snapshot.Snapshots.First().ManualSnapshot);
        mapped.Snapshots.First().Name.Should().Be(snapshot.Snapshots.First().Name);
        mapped.Snapshots.First().RawReferencedSize.Should().Be(snapshot.Snapshots.First().RawReferencedSize);
        mapped.Snapshots.First().RawSize.Should().Be(snapshot.Snapshots.First().RawSize);
        mapped.Snapshots.First().ReferencedSize.Should().Be(snapshot.Snapshots.First().ReferencedSize);
        mapped.Snapshots.First().Size.Should().Be(snapshot.Snapshots.First().Size);
        mapped.Snapshots.First().SmartFolderName.Should().Be(snapshot.Snapshots.First().SmartFolderName);
        mapped.Snapshots.First().SnapshotId.Should().Be(snapshot.Snapshots.First().SnapshotId);
    }


    [Fact]
    [Unit]
    public void SmartStorageSnapshotsResponseDto_Success_NullCollection()
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = new SmartStorageSnapshots()
        {
            AvailableSnapshots = 1,
            UsedSnapshots = 2,
            TotalSnapshots = 3,
            SnapshotTasks = null,
            Snapshots = null
        };

        var httpContext = new DefaultHttpContext();
        var mapped = snapshot.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.UsedSnapshots.Should().Be(snapshot.UsedSnapshots);
        mapped.AvailableSnapshots.Should().Be(snapshot.AvailableSnapshots);
        mapped.TotalSnapshots.Should().Be(snapshot.TotalSnapshots);
        mapped.SnapshotTasks.Should().HaveCount(0);
        mapped.Snapshots.Should().HaveCount(0);
    }

    #endregion

    #region Model

    [Fact]
    [Unit]
    public void SmartStorageCatalog_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "Category",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName = "DisplayName",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-Mode"},
                }
            }
        };
        var mapped = legacyCatalog.MapToSmartStorageCatalog(InternalSmartStorageCatalog);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Replica.Should().Be(InternalSmartStorageCatalog[0].Replica);
        mapped.First().Snapshot.Should().Be(InternalSmartStorageCatalog[0].Snapshot);
        mapped.First().Size.Should().Be(InternalSmartStorageCatalog[0].Size);
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_WithConfigs_Null_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "Category",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName = "DisplayName",
                BaseConfigProducts = null,
            }
        };
        InternalSmartStorageCatalog[0].Snapshot = null;
        InternalSmartStorageCatalog[0].Size = null;
        InternalSmartStorageCatalog[0].Replica = default;

        var mapped = legacyCatalog.MapToSmartStorageCatalog(InternalSmartStorageCatalog);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Replica.Should().Be(false);
        mapped.First().Snapshot.Should().Be(default);
        mapped.First().Size.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_WithConfigs_Empty_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "Category",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName = "DisplayName",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>(),
            }
        };
        InternalSmartStorageCatalog[0].Snapshot = "";
        InternalSmartStorageCatalog[0].Size = "";
        InternalSmartStorageCatalog[0].Replica = default;

        var mapped = legacyCatalog.MapToSmartStorageCatalog(InternalSmartStorageCatalog);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Replica.Should().Be(false);
        mapped.First().Snapshot.Should().Be(string.Empty);
        mapped.First().Size.Should().Be(string.Empty);
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_ProductCode_Null()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "Category",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "",
                SetupFeePrice = 3,
                DisplayName = "DisplayName",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(),
                }
            }
        };
        var mapped = legacyCatalog.MapToSmartStorageCatalog(InternalSmartStorageCatalog);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Replica.Should().Be(false);
        mapped.First().Snapshot.Should().BeNull();
        mapped.First().Size.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void SmartStorageCatalog_ProductCode_Invalid()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "Category",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCodeaa",
                SetupFeePrice = 3,
                DisplayName = "DisplayName",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode="dasdasds" }
                }
            }
        };
        var mapped = legacyCatalog.MapToSmartStorageCatalog(InternalSmartStorageCatalog);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Replica.Should().Be(false);
        mapped.First().Snapshot.Should().BeNull();
        mapped.First().Size.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void LegacySmartStorageListItem_To_SmartStorage()
    {
        var legacySmartStorageListItem = new LegacySmartStorageListItem()
        {
            Id = 123,
            Name = "Name",
            IncludedInFolders = new List<string>() { "folder1" },
            ExpirationDate = new DateTime(2026, 01, 30),
            Status = SmartStoragesStatuses.Active,
            Model = "model",
            ActivationDate = new DateTime(2025, 01, 10),
        };

        var mapped = legacySmartStorageListItem.MapToListitem("aru-25085", GetProject());
        mapped.Should().NotBeNull();
        mapped.Id.Should().Be(legacySmartStorageListItem.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Name.Should().Be(legacySmartStorageListItem.Name);
        mapped.Status.State.Should().Be(legacySmartStorageListItem.Status.ToString());
        mapped.Properties.Folders.Should().HaveCount(1);
        mapped.Properties.Folders.FirstOrDefault().Should().Be(legacySmartStorageListItem.IncludedInFolders.First());
        mapped.Properties.DueDate.Should().Be(legacySmartStorageListItem.ExpirationDate);
        mapped.Properties.ActivationDate.Should().Be(legacySmartStorageListItem.ActivationDate);
        mapped.Properties.Package.Should().Be(legacySmartStorageListItem.Model);
        mapped.Location.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void LegacySmartStorageDetail_To_SmartStorage()
    {
        var legacySmartStorageDetail = new LegacySmartStorageDetail()
        {
            Status = SmartStoragesStatuses.Active,
            Model = "model",
            CustomName = "admin",
            IpAddress = "127.0.0.1",
            MonthlyUnitPrice = 50.0m,
            ActivationDate = new DateTime(2025, 01, 10),
            AutoRenewEnabled = true,
            RenewAllowed = true,
            Components = new List<LegacyComponent>()
            {
                new LegacyComponent()
                {
                    Name = "component-name",
                    Quantity = 2
                }
            },
            Id = 123,
            Name = "Name",
            IncludedInFolders = new List<string>() { "folder1" },
            ExpirationDate = new DateTime(2026, 01, 30),
        };

        var mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Name.Should().Be(legacySmartStorageDetail.CustomName);
        mapped.Location.Should().BeNull();
        mapped.Id.Should().Be(legacySmartStorageDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version!.Data!.Current.Should().Be(1);
        mapped.Uri.Should().NotBeNull();
        mapped.Properties.Package.Should().Be(legacySmartStorageDetail.Model);
        mapped.Properties.ActivationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Properties.Folders.Should().HaveCount(1);
        mapped.Properties.Folders.FirstOrDefault().Should().Be(legacySmartStorageDetail.IncludedInFolders.First());
        mapped.Properties.DueDate.Should().Be(legacySmartStorageDetail.ExpirationDate);
    }



    //[Fact]
    //[Unit]
    //public void LegacySmartStorageDetail_To_SmartStorage_Statuses()
    //{
    //    var legacySmartStorageDetail = new LegacySmartStorageDetail()
    //    {
    //        Status = SmartStoragesStatuses.Active,
    //        Model = "model",
    //        CustomName = "admin",
    //        IpAddress = "127.0.0.1",
    //        MonthlyUnitPrice = 50.0m,
    //        ActivationDate = new DateTime(2025, 01, 10),
    //        AutoRenewEnabled = true,
    //        RenewAllowed = true,
    //        SmartStorageInfo = new LegacySmartStorageInfo()
    //        {
    //            Status = SmartStorageInfoStatus.Creating
    //        },
    //        Components = new List<LegacyComponent>()
    //        {
    //            new LegacyComponent()
    //            {
    //                Name = "component-name",
    //                Quantity = 2
    //            }
    //        },
    //        Id = 123,
    //        Name = "Name",
    //        IncludedInFolders = new List<string>() { "folder1" },
    //        ExpirationDate = new DateTime(2026, 01, 30),
    //    };

    //    legacySmartStorageDetail.FirstSetupDone = false;
    //    var mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.ToBeActivated.Value);

    //    legacySmartStorageDetail.FirstSetupDone = true;
    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Active;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.Active.Value);

    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Deleted;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.Deleted.Value);

    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Maintenance;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.InMaintenance.Value);

    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Suspend;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.Suspended.Value);

    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Unavailable;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.Unavailable.Value);

    //    legacySmartStorageDetail.SmartStorageInfo.Status = SmartStorageInfoStatus.Upgrading;
    //    mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
    //    mapped.Status!.State.Should().Be(StatusValues.Upgrading.Value);
    //}

    [Fact]
    [Unit]
    public void LegacySmartStorageDetail_To_SmartStorage_NoCustomName()
    {
        var legacySmartStorageDetail = new LegacySmartStorageDetail()
        {
            Status = SmartStoragesStatuses.Active,
            Model = "model",
            IpAddress = "127.0.0.1",
            MonthlyUnitPrice = 50.0m,
            ActivationDate = new DateTime(2025, 01, 10),
            AutoRenewEnabled = true,
            RenewAllowed = true,
            Components = new List<LegacyComponent>()
            {
                new LegacyComponent()
                {
                    Name = "component-name",
                    Quantity = 2
                }
            },
            Id = 123,
            Name = "Name",
            IncludedInFolders = new List<string>() { "folder1" },
            ExpirationDate = new DateTime(2026, 01, 30),
        };

        var mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Name.Should().Be(legacySmartStorageDetail.Name);
        mapped.Location.Should().BeNull();
        mapped.Id.Should().Be(legacySmartStorageDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version.Data.Current.Should().Be(1);
        mapped.Uri.Should().NotBeNull();
        mapped.Properties.Package.Should().Be(legacySmartStorageDetail.Model);
        mapped.Properties.ActivationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Properties.Folders.Should().HaveCount(1);
        mapped.Properties.Folders.FirstOrDefault().Should().Be(legacySmartStorageDetail.IncludedInFolders.First());
        mapped.Properties.DueDate.Should().Be(legacySmartStorageDetail.ExpirationDate);
    }


    [Fact]
    [Unit]
    public void LegacySmartStorageDetail_To_SmartStorage_IncludedInFolder_null()
    {
        var legacySmartStorageDetail = new LegacySmartStorageDetail()
        {
            Status = SmartStoragesStatuses.Active,
            Model = "model",
            IpAddress = "127.0.0.1",
            MonthlyUnitPrice = 50.0m,
            ActivationDate = new DateTime(2025, 01, 10),
            AutoRenewEnabled = true,
            RenewAllowed = true,
            Components = new List<LegacyComponent>()
            {
                new LegacyComponent()
                {
                    Name = "component-name",
                    Quantity = 2
                }
            },
            Id = 123,
            Name = "Name",
            IncludedInFolders = null,
            ExpirationDate = new DateTime(2026, 01, 30),
        };

        var mapped = legacySmartStorageDetail.MapToDetail("aru-25085", GetProject(), false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Name.Should().Be(legacySmartStorageDetail.Name);
        mapped.Location.Should().BeNull();
        mapped.Id.Should().Be(legacySmartStorageDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version.Data.Current.Should().Be(1);
        mapped.Properties.Folders.Should().HaveCount(0);
        mapped.Uri.Should().NotBeNull();
        mapped.Properties.Package.Should().Be(legacySmartStorageDetail.Model);
        mapped.Properties.ActivationDate.Should().Be(legacySmartStorageDetail.ActivationDate);
        mapped.Properties.DueDate.Should().Be(legacySmartStorageDetail.ExpirationDate);
    }

    [Fact]
    [Unit]
    public void SmartStorageFolders_Success()
    {
        var legacySmartFolders = new List<LegacySmartFoldersItem>()
        {
            new LegacySmartFoldersItem()
            {
                 Name = "f1",
                 UsedSpace = "0GB",
                 Readonly = true,
                 AvailableSpace = "1GB",
                 IsRootFolder = true,
                 Position = "1",
                 PositionDisplay = "1",
                 RawAvailableSpace = 1000000000,
                 RawUsedSpace = 0
            }
        };
        var mapped = legacySmartFolders.MapToSmartStorageFolders();
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().Name.Should().Be(legacySmartFolders[0].Name);
        mapped.First().UsedSpace.Should().Be(legacySmartFolders[0].UsedSpace);
        mapped.First().Readonly.Should().Be(legacySmartFolders[0].Readonly);
        mapped.First().AvailableSpace.Should().Be(legacySmartFolders[0].AvailableSpace);
        mapped.First().IsRootFolder.Should().Be(legacySmartFolders[0].IsRootFolder);
        mapped.First().PositionDisplay.Should().Be(legacySmartFolders[0].PositionDisplay);
        mapped.First().SmartFolderID.Should().Be(legacySmartFolders[0].SmartFolderID);
    }

    [Fact]
    [Unit]
    public void MapProtocolList_Success()
    {
        var legacyProtocols = new List<LegacyProtocol>()
        {
            new LegacyProtocol()
            {
                ServiceType = LegacyServiceType.Samba,
                ServiceStatus = LegacyServiceStatus.Running,
            },
            new LegacyProtocol()
            {
                ServiceType = LegacyServiceType.WebDav,
                ServiceStatus = LegacyServiceStatus.Stopped,
            },
            new LegacyProtocol()
            {
                ServiceType = LegacyServiceType.Ssh,
                ServiceStatus = LegacyServiceStatus.Unknown,
            },
            new LegacyProtocol()
            {
                ServiceType = LegacyServiceType.ExternalReachability,
                ServiceStatus = LegacyServiceStatus.Unknown,
                Error = true,
                ErrorMessage = "An error occured"
            }
        };

        var mapped = legacyProtocols.MapProtocolList().ToList();
        mapped.Should().NotBeNull().And.HaveCount(4);

        mapped[0].ServiceType.Should().Be(ServiceType.Samba);
        mapped[0].ServiceStatus.Should().Be(ServiceStatus.Running);
        mapped[0].Error.Should().BeFalse();
        mapped[0].ErrorMessage.Should().BeNullOrEmpty();

        mapped[1].ServiceType.Should().Be(ServiceType.WebDav);
        mapped[1].ServiceStatus.Should().Be(ServiceStatus.Stopped);
        mapped[1].Error.Should().BeFalse();
        mapped[1].ErrorMessage.Should().BeNullOrEmpty();

        mapped[2].ServiceType.Should().Be(ServiceType.Ssh);
        mapped[2].ServiceStatus.Should().Be(ServiceStatus.Unknown);
        mapped[2].Error.Should().BeFalse();
        mapped[2].ErrorMessage.Should().BeNullOrEmpty();

        mapped[3].ServiceType.Should().Be(ServiceType.ExternalReachability);
        mapped[3].ServiceStatus.Should().Be(ServiceStatus.Unknown);
        mapped[3].Error.Should().BeTrue();
        mapped[3].ErrorMessage.Should().Be("An error occured");
    }

    [Fact]
    [Unit]
    public void MapStatistics_Null()
    {
        var mapped = ((LegacyStatistics)null).MapStatistics();
        mapped.Should().NotBeNull();
        mapped.SmartFolders.Should().BeEmpty();
        mapped.Snapshots.Should().BeEmpty();
        mapped.TotalSmartFolders.Should().Be(0);
        mapped.TotalSmartFoldersSize.Should().BeNull();
        mapped.TotalSnapshots.Should().Be(0);
        mapped.TotalSnapshotsSize.Should().BeNull();
        mapped.TotalDiskSpace.Should().BeNull();
        mapped.AvailableDiskSpace.Should().BeNull();
        mapped.ReservedDiskSpace.Should().BeNull();
        mapped.TotalUsedSpace.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void MapStatistics_FullData()
    {
        var legacyStats = new LegacyStatistics()
        {
            SmartFolders = new List<LegacyDataSet>
            {
                new LegacyDataSet{
                    Name ="TestDarioPM",
                    Size= "491MB",
                    RawSize= 514547712
                },
                new LegacyDataSet{
                    Name = "SSU99268710",
                    Size= "124KB",
                    RawSize= 126976
                },
                new LegacyDataSet{
                    Name= "TestDarioDEV",
                    Size= "96KB",
                    RawSize= 98304
                },
                new LegacyDataSet{
                    Name =  "TestFrancescoDev",
                    Size = "96KB",
                    RawSize= 98304
                }
            },
            Snapshots = new List<LegacySnapshot>{
                new LegacySnapshot{
                    Name= "manual_2025_03_04_11_18_11",
                    Size= "64.00KB",
                    SmartFolderName= "TestDarioDEV",
                    RawSize= 65536,
                    ReferencedSize= "96.00KB",
                    RawReferencedSize= 98304
                },
                new LegacySnapshot{
                    Name= "manual_2025_03_07_05_05_18",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                },
                new LegacySnapshot{
                    Name= "manual_2025_04_07_01_28_53",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                },
                new LegacySnapshot{
                    Name= "manual_2025_04_08_10_01_32",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                }
            },
            TotalSmartFolders = 4,
            RawTotalSmartFoldersSize = 514871296,
            TotalSmartFoldersSize = "491.02MB",
            TotalSnapshots = 4,
            RawTotalSnapshotsSize = 65536,
            TotalSnapshotsSize = "64.00KB",
            RawTotalDiskSpace = 10737418240,
            TotalDiskSpace = "10.00GB",
            RawAvailableDiskSpace = 6986108928,
            AvailableDiskSpace = "6.51GB",
            RawReservedDiskSpace = 0,
            ReservedDiskSpace = "0.00KB",
            TotalUsedSpace = "3.49GB",
            RawTotalUsedSpace = 3751309312
        };

        var mapped = legacyStats.MapStatistics();
        mapped.Should().NotBeNull();
        mapped.SmartFolders.Should().HaveCount(4);
        mapped.SmartFolders.First().Name.Should().Be("TestDarioPM");
        mapped.SmartFolders.First().Size.Size.Should().Be("491MB");
        mapped.SmartFolders.First().Size.RawSize.Should().Be(514547712);

        mapped.Snapshots.Should().HaveCount(4);
        mapped.Snapshots.First().Name.Should().Be("manual_2025_03_04_11_18_11");
        mapped.Snapshots.First().Size.Size.Should().Be("96.00KB");
        mapped.Snapshots.First().Size.RawSize.Should().Be(98304);
        mapped.Snapshots.First().ReferencedSize.Size.Should().Be("96.00KB");
        mapped.Snapshots.First().ReferencedSize.RawSize.Should().Be(98304);
        mapped.TotalSmartFolders.Should().Be(4);
        mapped.TotalSmartFoldersSize.Size.Should().Be("491.02MB");
        mapped.TotalSmartFoldersSize.RawSize.Should().Be(514871296);
        mapped.TotalSnapshots.Should().Be(4);
        mapped.TotalSnapshotsSize.RawSize.Should().Be(65536);
        mapped.TotalSnapshotsSize.Size.Should().Be("64.00KB");
        mapped.TotalDiskSpace.RawSize.Should().Be(10737418240);
        mapped.TotalDiskSpace.Size.Should().Be("10.00GB");
        mapped.AvailableDiskSpace.Size.Should().Be("6.51GB");
        mapped.AvailableDiskSpace.RawSize.Should().Be(6986108928);
        mapped.ReservedDiskSpace.Size.Should().Be("0.00KB");
        mapped.ReservedDiskSpace.RawSize.Should().Be(0);
        mapped.TotalUsedSpace.RawSize.Should().Be(3751309312);
        mapped.TotalUsedSpace.Size.Should().Be("3.49GB");
    }

    [Fact]
    [Unit]
    public void MapSnapshots_Null()
    {
        var mapped = ((LegacySnapshots)null).MapSnapshots();
        mapped.Should().NotBeNull();
        mapped.UsedSnapshots.Should().Be(0);
        mapped.TotalSnapshots.Should().Be(0);
        mapped.AvailableSnapshots.Should().Be(0);
        mapped.Snapshots.Should().BeEmpty();
        mapped.SnapshotTasks.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public void MapSnapshots_FullData()
    {
        var legacySnapshots = new LegacySnapshots()
        {
            Snapshots = new List<LegacyManualSnapshots>()
              {
                  new LegacyManualSnapshots()
                  {
                      Name= "snapshot1",
                      CreationDate = DateTime.UtcNow,
                      ManualSnapshot = true,
                      RawSize = 10000,
                      RawReferencedSize = 0,
                      Size = "10KB",
                      ReferencedSize ="0",
                      SmartFolderName ="f1",
                      SnapshotId = "f1/08_04_2025"
                  }
              },
            SnapshotTasks = new List<LegacySnapshotsTasks>()
             {
                 new LegacySnapshotsTasks()
                 {
                      CreationDate = DateTimeOffset.UtcNow,
                      SmartFolderName = "f2",
                      Enabled = true,
                      Iterations =2,
                      Schedule = new LegacySchedule(),
                      ScheduleType =  "type1",
                      SnapshotTaskId = 123,
                 }
             },
            TotalSnapshots = 1,
            UsedSnapshots = 1,
            AvailableSnapshots = 1,
        };

        var mapped = legacySnapshots.MapSnapshots();
        mapped.Should().NotBeNull();
        mapped.UsedSnapshots.Should().Be(1);
        mapped.TotalSnapshots.Should().Be(1);
        mapped.AvailableSnapshots.Should().Be(1);
        mapped.Snapshots.Should().HaveCount(1);
        mapped.SnapshotTasks.Should().HaveCount(1);
    }

    #endregion

    #region Utils
    private static SmartStorage SmartStorageModel =
                 new SmartStorage()
                 {
                     Id = "123",
                     Category = new Abstractions.Models.Category()
                     {
                         Name = "Baremetal Network",
                         Provider = "Aruba.Baremetal",
                         Typology = new Abstractions.Models.Typology()
                         {
                             Id = "baremetal",
                             Name = "baremetal",
                         }
                     },
                     Name = "name-test",
                     CreatedBy = "aru-25085",
                     CreationDate = DateTimeOffset.UtcNow,
                     Location = GetLocationModel(),
                     Status = new Abstractions.Models.Status()
                     {
                         State = StatusValues.Active.Value,
                         CreationDate = DateTimeOffset.UtcNow,
                     },
                     Uri = "baremetal/123",
                     Ecommerce = new Ecommerce(),
                     LinkedResources = new System.Collections.ObjectModel.Collection<LinkedResource>(),
                     Project = new Project()
                     {
                         Name = "123-default",
                         Id = "123",
                     },
                     Tags = new System.Collections.ObjectModel.Collection<string> { "tag1" },
                     Properties = GetSmartStoragePropertiesModel(),
                     Version = new Abstractions.Models.Version()
                     {
                         Data = new DataVersion()
                         {
                             Current = 1,
                             Previous = null
                         },
                         Model = "version-model"
                     },
                     UpdatedBy = "aru-25085",
                 };

    private static Abstractions.Models.Location GetLocationModel() => new Abstractions.Models.Location()
    {
        City = "Bergamo",
        Code = "ITBG",
        Value = "ITBG-Bergamo"
    };

    private static Abstractions.Providers.Models.Projects.Project GetProject() => new Abstractions.Providers.Models.Projects.Project()
    {
        Metadata = new Abstractions.Providers.Models.Projects.ProjectMetadata()
        {
            Name = "123-default",
            Id = "123",
        }
    };

    private static SmartStorageProperties GetSmartStoragePropertiesModel() => new SmartStorageProperties()
    {
        RenewAllowed = true,
        ActivationDate = DateTimeOffset.UtcNow,
        AutoRenewEnabled = true,
        DueDate = DateTimeOffset.UtcNow,
        MonthlyUnitPrice = 50.00m,
        Username = "Username",
        AutoRenewDeviceId = "123",
        Folders = new string[] { "root", "f1" },
        IsFirstSetupDone = true,
        Package = "package1",
        Replica = true,
        Samba = "samba",
        Server = "server",
        UpgradeAllowed = true
    };

    private List<InternalSmartStorageCatalog> InternalSmartStorageCatalog = new List<InternalSmartStorageCatalog>()
    {
        new InternalSmartStorageCatalog()
        {
            Code = "ProductCode",
            Size = "10GB",
            Replica = true,
            Snapshot = "1"
        }
    };
    #endregion
}
