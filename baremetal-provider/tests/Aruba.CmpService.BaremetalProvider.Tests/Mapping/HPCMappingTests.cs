
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
using FluentAssertions;
using Moq;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;
public class HPCMappingTests : TestBase
{
    public HPCMappingTests(ITestOutputHelper output) : base(output)
    { }

    #region MAPS

    [Fact]
    [Unit]
    public void MapToResponse_Dto_Full()
    {
        HPC input = CreateHPC();
        var ret = input.MapToResponse();

        ret.Should().NotBeNull();
        ret.Metadata.Id.Should().Be(input.Id);
        ret.Metadata.Name.Should().Be(input.Name);
        ret.Metadata.Uri.Should().Be(input.Uri);
        ret.Metadata.CreatedBy.Should().Be(input.CreatedBy);
        ret.Metadata.UpdatedBy.Should().Be(input.CreatedBy);
        ret.Metadata.CreationDate.Should().Be(input.CreationDate);
        ret.Metadata.UpdateDate.Should().Be(input.UpdateDate);
        ret.Metadata.Category.Name.Should().Be(input.Category?.Name);
        ret.Metadata.Category.Provider.Should().Be(input.Category?.Provider);
        ret.Metadata.Category.Typology.Should().NotBeNull();
        ret.Metadata.Category.Typology.Id.Should().Be(input.Category?.Typology?.Id);
        ret.Metadata.Category.Typology.Name.Should().Be(input.Category?.Typology?.Name);
        ret.Metadata.Location.Name.Should().Be(input.Location?.Name);
        ret.Metadata.Location.Value.Should().Be(input.Location?.Value);
        ret.Metadata.Project.Id.Should().Be(input.Project.Id);
        ret.Properties.DueDate.Should().Be(input.Properties?.DueDate);
        ret.Properties.ActivationDate.Should().Be(input.Properties?.ActivationDate);
        ret.Properties.RenewAllowed.Should().Be(input.Properties?.RenewAllowed ?? false);
        ret.Properties.AutoRenewDeviceId.Should().Be(input.Properties?.AutoRenewDeviceId);
        ret.Properties.AutoRenewEnabled.Should().Be(input.Properties?.AutoRenewEnabled ?? false);
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(1);
        ret.Status.State.Should().Be(input.Status?.State);
        ret.MonthlyUnitPrice.Should().Be(input.MonthlyUnitPrice);
        ret.NumServices.Should().Be(input.NumServices);
        ret.Services.Should().HaveCount(4);
        ret.Services[0].HPCServiceType.Should().Be(BundleServiceModuleType.Server);
        ret.Services[3].HPCServiceType.Should().Be(BundleServiceModuleType.Firewall);
    }

    [Fact]
    [Unit]
    public void MapToResponse_Dto_EmptyFolder()
    {
        HPC input = CreateHPC(true);
        var ret = input.MapToResponse();
        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(0);
    }

    [Fact]
    [Unit]
    public void MapToResponse_Dto_NullFolder()
    {
        HPC input = CreateHPC(false, true);
        var ret = input.MapToResponse();
        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void MapLegacyList_Full()
    {
        var input = CreateLegacyHPCListItem();
        var legacyProject = CreateLegacyProject();
        var location = CreateLocation();
        var ret = input.MapToListitem("ARU-25198", legacyProject, location);

        ret.Should().NotBeNull();
        ret.Id.Should().Be(input.Id.ToString());
        ret.Name.Should().Be(input.Name);
        ret.Uri.Should().Be($"/projects/{legacyProject.Metadata.Id}/providers/Aruba.Baremetal/mcis/{input.Id}");
        ret.CreatedBy.Should().Be("ARU-25198");
        ret.UpdatedBy.Should().Be("ARU-25198");
        ret.CreationDate.Should().Be(input.ActivationDate);
        ret.UpdateDate.Should().Be(input.ActivationDate);
        ret.Category.Name.Should().Be(Abstractions.Constants.Categories.MetalCloudInfrastructure.Value);
        ret.Category.Provider.Should().Be("Aruba.Baremetal");
        ret.Category.Typology.Should().NotBeNull();
        ret.Category.Typology.Id.Should().Be(Typologies.HPC.Value);
        ret.Category.Typology.Name.Should().Be(Typologies.HPC.Value.ToUpperInvariant());
        ret.Location.Name.Should().Be(location.Name);
        ret.Location.Value.Should().Be(location.Value);
        ret.Project.Id.Should().Be(legacyProject.Metadata.Id);
        ret.Project.Name.Should().Be(legacyProject.Metadata.Name);
        ret.Properties.DueDate.Should().Be(input.ExpirationDate);
        ret.Properties.ActivationDate.Should().Be(input.ActivationDate);
        ret.Properties.Name.Should().Be(input.Name);
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(1);
        ret.Status.State.Should().Be(input.Status.ToString());
        ret.Status.CreationDate.Should().Be(input.ActivationDate);
        ret.Services.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void MapLegacyList_EmptyFolders()
    {
        var input = CreateLegacyHPCListItem(true);
        var legacyProject = CreateLegacyProject();
        var location = CreateLocation();
        var ret = input.MapToListitem("ARU-25198", legacyProject, location);

        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(0);
    }

    [Fact]
    [Unit]
    public void MapLegacyList_NullFolders()
    {
        var input = CreateLegacyHPCListItem(false, true);
        var legacyProject = CreateLegacyProject();
        var location = CreateLocation();
        var ret = input.MapToListitem("ARU-25198", legacyProject, location);

        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void MapToDetail_Full()
    {
        var input = CreateLegacyHPCDetail();
        var ret = input.MapToDetail(CreateLocation(), CreatePRJ());

        ret.Should().NotBeNull();
        ret.Id.Should().Be(input.Id.ToString());
        ret.Name.Should().Be(input.Name);
        ret.CreationDate.Should().Be(input.ActivationDate);
        ret.Properties.DueDate.Should().Be(input.ExpirationDate);
        ret.Properties.ActivationDate.Should().Be(input.ActivationDate);
        ret.Properties.Name.Should().Be(input.Name);
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(1);
        ret.Status.State.Should().Be(input.Status.ToString());
        ret.Status.CreationDate.Should().Be(input.ActivationDate);
        ret.MonthlyUnitPrice.Should().Be(input.MonthlyUnitPrice);
        ret.Services.Should().HaveCount(4);
        ret.Services[0].ServiceType.Should().Be(BundleServiceModuleType.Server);
        ret.Services[3].ServiceType.Should().Be(BundleServiceModuleType.Firewall);
    }

    [Fact]
    [Unit]
    public void MapToDetail_EmptyFolders()
    {
        var input = CreateLegacyHPCDetail(true);
        var ret = input.MapToDetail(CreateLocation(), CreatePRJ());

        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().NotBeNull().And.HaveCount(0);
    }

    [Fact]
    [Unit]
    public void MapToDetail_NullFolders()
    {
        var input = CreateLegacyHPCDetail(false, true);
        var ret = input.MapToDetail(CreateLocation(), CreatePRJ());

        ret.Should().NotBeNull();
        ret.Properties.Folders.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void MapToListResponse_Content()
    {
        var input = CreateHPC();
        var ret = input.MapToListResponse();

        ret.Should().NotBeNull();
        ret.TotalCount.Should().Be(4);
        ret.Values[0].HPCServiceType.Should().Be(BundleServiceModuleType.Server);
        ret.Values[3].HPCServiceType.Should().Be(BundleServiceModuleType.Firewall);
    }

    [Fact]
    [Unit]
    public void MapToBaremetalResource()
    {
        var input = CreateHPC();
        var ret = input.Map(It.IsAny<AutorenewFolderAction>());

        ret.Should().NotBeNull();
        ret.Id.Should().Be(input.Id?.ToString());
        ret.Name.Should().Be(input.Name);
        ret.Uri.Should().Be(input.Uri);
        ret.CreatedBy.Should().Be("ARU-25198");
        ret.UpdatedBy.Should().Be("ARU-25198");
        ret.CreationDate.Should().Be(input.CreationDate);
        ret.UpdateDate.Should().Be(input.UpdateDate);
        ret.Category.Should().NotBeNull();
        ret.Category.Name.Should().Be(Abstractions.Constants.Categories.MetalCloudInfrastructure.Value);
        ret.Category.Typology.Should().NotBeNull();
        ret.Category.Typology.Id.Should().Be(Typologies.HPC.Value);
        ret.Category.Typology.Name.Should().Be(Typologies.HPC.Value.ToUpperInvariant());
        ret.Location.Should().NotBeNull();
        ret.Location.Name.Should().Be(input.Location?.Name);
        ret.Location.Value.Should().Be(input.Location?.Value);
        ret.Project.Should().NotBeNull();
        ret.Project.Id.Should().Be(input.Project?.Id);
        ret.Status.Should().NotBeNull();
        ret.Status.State.Should().Be(input.Status?.State);
        ret.Status.CreationDate.Should().Be(input.Status?.CreationDate);
    }

    #endregion

    #region UTILS

    private static HPC CreateHPC(bool emptyFolder = false, bool nullFolder = false)
    {
        return new HPC()
        {
            Id = "123",
            Location = CreateLocation(),
            Category = CreateCategory(),
            CreatedBy = "ARU-25198",
            CreationDate = DateTimeOffset.Now,
            Name = "HPC1",
            Status = CreateStatus(),
            Project = CreateProject(),
            UpdatedBy = "ARU-25198",
            UpdateDate = DateTimeOffset.Now,
            Properties = CreateHPCProperties(emptyFolder, nullFolder),
            NumServices = 4,
            Services = CreateHPCContent(),
            MonthlyUnitPrice = 10
        };
    }

    private static Abstractions.Models.Project CreateProject()
    {
        return new Abstractions.Models.Project()
        {
            Id = "679a59350c011157344272a8",
            Name = "Project"
        };
    }

    private static Abstractions.Providers.Models.Projects.Project CreatePRJ()
    {
        return new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new Abstractions.Providers.Models.Projects.ProjectMetadata()
            {
                Id = "679a59350c011157344272a8",
                Name = "Project"
            }
        };
    }

    private static Status CreateStatus()
    {
        return new Status()
        {
            CreationDate = DateTimeOffset.Now,
            State = StatusValues.Active.Value
        };
    }

    private static HPCProperties CreateHPCProperties(bool emptyFolder = false, bool nullFolder = false)
    {
        var ret = new HPCProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            DueDate = DateTimeOffset.Now,
            Name = "Name",
            Folders = ["Folder1"]
        };

        if (emptyFolder) ret.Folders = [];

        if (nullFolder) ret.Folders = null;

        return ret;
    }

    private static Location CreateLocation()
    {
        return new Location()
        {
            Name = "Name",
            Value = "Value"
        };
    }

    private static Category CreateCategory()
    {
        return new Category()
        {
            Name = "MetalCloudInfrastructure",
            Provider = "Baremetal",
            Typology = new Typology()
            {
                Id = "hpc",
                Name = "HPC"
            }
        };
    }

    private static List<HPCBundleContent> CreateHPCContent()
    {
        var ret = new List<HPCBundleContent>();

        ret.Add(new HPCBundleContent()
        {
            ServiceID = 1,
            ServiceName = "TestService1",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new HPCBundleContent()
        {
            ServiceID = 2,
            ServiceName = "TestService2",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new HPCBundleContent()
        {
            ServiceID = 3,
            ServiceName = "TestService3",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new HPCBundleContent()
        {
            ServiceID = 4,
            ServiceName = "TestService4",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Firewall
        });

        return ret;
    }

    private static List<LegacyBundleService> CreateLegacyHPCContent()
    {
        var ret = new List<LegacyBundleService>();

        ret.Add(new LegacyBundleService()
        {
            ServiceID = 1,
            ServiceName = "TestService1",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new LegacyBundleService()
        {
            ServiceID = 2,
            ServiceName = "TestService2",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new LegacyBundleService()
        {
            ServiceID = 3,
            ServiceName = "TestService3",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Server
        });

        ret.Add(new LegacyBundleService()
        {
            ServiceID = 4,
            ServiceName = "TestService4",
            ServiceStatus = BundleServiceStatuses.Active,
            ServiceType = BundleServiceModuleType.Firewall
        });

        return ret;
    }

    private LegacyHPCListItem CreateLegacyHPCListItem(bool emptyFolder = false, bool nullFolder = false)
    {
        var ret = new LegacyHPCListItem()
        {
            Id = 1,
            Name = "Name",
            ActivationDate = DateTime.Now,
            ExpirationDate = DateTime.Now,
            IncludedInFolders = ["Folder1"],
            Status = HPCStatuses.Active,
        };

        if (emptyFolder) ret.IncludedInFolders = [];

        if (nullFolder) ret.IncludedInFolders = null;

        return ret;
    }

    private Abstractions.Providers.Models.Projects.Project CreateLegacyProject()
    {
        return new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new Abstractions.Providers.Models.Projects.ProjectMetadata()
            {
                Id = "5",
                Name = "LegacyProjectName",
            }
        };
    }

    private LegacyHPCDetail CreateLegacyHPCDetail(bool emptyFolder = false, bool nullFolder = false)
    {
        var ret = new LegacyHPCDetail()
        {
            Id = 1,
            Status = HPCStatuses.Active,
            MonthlyUnitPrice = 50,
            ActivationDate = DateTime.Now,
            ExpirationDate = DateTime.Now,
            IncludedInFolders = ["Folder1"],
            Name = "Name",
            BundleContent = CreateLegacyHPCContent(),
        };

        if (emptyFolder) ret.IncludedInFolders = [];

        if (nullFolder) ret.IncludedInFolders = null;

        return ret;
    }

    #endregion
}
