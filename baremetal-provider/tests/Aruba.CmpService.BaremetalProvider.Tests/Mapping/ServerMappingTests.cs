using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;
public class ServerMappingTests : TestBase
{
    public ServerMappingTests(ITestOutputHelper output)
        : base(output)
    {
    }

    #region Response
    [Fact]
    [Unit]
    public void ServerCatalog_Success_EmptyItems()
    {
        var catalog = new Abstractions.Models.Servers.ServerCatalog();
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);


        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(0);
        mapped.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_Success_WithItems()
    {
        var catalog = new Abstractions.Models.Servers.ServerCatalog()
        {
            TotalCount = 1,
            Values = new List<ServerCatalogItem>()
            {
                new ServerCatalogItem()
                {
                    Category = "Category",
                    Name = "Name",
                    Code = "Code",
                    Connectivity = "Connectivity",
                    DiscountedPrice = 1,
                    Hdd = "Hdd",
                    IsSoldOut = true,
                    Location = "Location",
                    Price = 2,
                    SetupFeePrice = 3,
                    Processor = "Processor",
                    Ram = "Ram"
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
        mapped.Values[0].Connectivity.Should().Be(catalog.Values.First().Connectivity);
        mapped.Values[0].DiscountedPrice.Should().Be(catalog.Values.First().DiscountedPrice);
        mapped.Values[0].Hdd.Should().Be(catalog.Values.First().Hdd);
        mapped.Values[0].IsSoldOut.Should().Be(catalog.Values.First().IsSoldOut);
        mapped.Values[0].Location.Should().Be(catalog.Values.First().Location);
        mapped.Values[0].Price.Should().Be(catalog.Values.First().Price);
        mapped.Values[0].SetupFeePrice.Should().Be(catalog.Values.First().SetupFeePrice);
        mapped.Values[0].Processor.Should().Be(catalog.Values.First().Processor);
        mapped.Values[0].Ram.Should().Be(catalog.Values.First().Ram);
    }


    [Fact]
    [Unit]
    public void ServerCatalog_Success_With1ItemsNULL()
    {
        var catalog = new Abstractions.Models.Servers.ServerCatalog()
        {
            TotalCount = 1,
            Values = new List<ServerCatalogItem>()
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
    public void Server_Success_NotNull()
    {
        var server = CreateServer();
        server.Properties = new ServerProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            Gpu = "Gpu",
            AutoRenewEnabled = true,
            Components = new List<ServerComponent>() { new ServerComponent() { Name = "Name", Quantity = 1 } },
            DueDate = DateTimeOffset.Now,
            Folders = new List<string>() { "Folder1" },
            Hdd = "Hdd",
            IpAddress = "127.0.0.1",
            Model = "Model",
            MonthlyUnitPrice = 2,
            OperatingSystem = "OperatingSystem",
            PleskLicense = new ServerPleskLicense()
            {
                ActivationDate = DateTimeOffset.Now,
                ActivationCode = "ActivationCode",
                Code = "Code",
                Description = "Description",
                ServerIp = "127.0.0.1",
                Addons = new List<ServerPleskLicenseAddon>() {
                    new ServerPleskLicenseAddon
                    {
                        ActivationDate = DateTimeOffset.Now,
                        Code = "Code",
                        Description = "Description"
                    }
                }
            },
            Processor = "Processor",
            Ram = "Ram",
            RenewAllowed = false,
            RenewUpgradeAllowed = false,
            UpgradeAllowed = false,
        };
        server.UpdatedBy = "AM";

        var mapped = server.MapToResponse();

        CommonMappingTests.ValidateBaseResource<Server, ServerResponseDto, ServerPropertiesResponseDto>(server, mapped, server.UpdatedBy);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Properties.Model);
        mapped?.Properties?.Processor.Should().Be(server.Properties.Processor);
        mapped?.Properties?.OperatingSystem.Should().Be(server.Properties.OperatingSystem);
        mapped?.Properties?.Ram.Should().Be(server.Properties.Ram);
        mapped?.Properties?.Gpu.Should().Be(server.Properties.Gpu);
        mapped?.Properties?.Hdd.Should().Be(server.Properties.Hdd);
        mapped?.Properties?.IpAddress.Should().Be(server.Properties.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.Properties.DueDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.Properties.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.Properties.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.Properties.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.Properties.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.Properties.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.Properties.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.Properties.Folders.Count()).And.HaveElementAt(0, server.Properties.Folders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Properties.Components.Count());
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.Properties.PleskLicense.Code);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.Properties.PleskLicense.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.Properties.PleskLicense.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.Properties.PleskLicense.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.Properties.PleskLicense.ServerIp);
        mapped?.Properties?.PleskLicense?.Addons.Should().HaveCount(1);
    }

    [Fact]
    [Unit]

    public void Server_Success_Addons_Null()
    {
        var server = CreateServer();
        server.Properties = new ServerProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            Gpu = "Gpu",
            AutoRenewEnabled = true,
            Components = new List<ServerComponent>() { new ServerComponent() { Name = "Name", Quantity = 1 } },
            DueDate = DateTimeOffset.Now,
            Folders = new List<string>() { "Folder1" },
            Hdd = "Hdd",
            IpAddress = "127.0.0.1",
            Model = "Model",
            MonthlyUnitPrice = 2,
            OperatingSystem = "OperatingSystem",
            PleskLicense = new ServerPleskLicense()
            {
                ActivationDate = DateTimeOffset.Now,
                ActivationCode = "ActivationCode",
                Code = "Code",
                Description = "Description",
                ServerIp = "127.0.0.1",
                Addons = null
            },
            Processor = "Processor",
            Ram = "Ram",
            RenewAllowed = false,
            RenewUpgradeAllowed = false,
            UpgradeAllowed = false,
        };
        server.UpdatedBy = "AM";

        var mapped = server.MapToResponse();

        CommonMappingTests.ValidateBaseResource<Server, ServerResponseDto, ServerPropertiesResponseDto>(server, mapped, server.UpdatedBy);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Properties.Model);
        mapped?.Properties?.Processor.Should().Be(server.Properties.Processor);
        mapped?.Properties?.OperatingSystem.Should().Be(server.Properties.OperatingSystem);
        mapped?.Properties?.Ram.Should().Be(server.Properties.Ram);
        mapped?.Properties?.Gpu.Should().Be(server.Properties.Gpu);
        mapped?.Properties?.Hdd.Should().Be(server.Properties.Hdd);
        mapped?.Properties?.IpAddress.Should().Be(server.Properties.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.Properties.DueDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.Properties.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.Properties.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.Properties.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.Properties.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.Properties.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.Properties.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.Properties.Folders.Count()).And.HaveElementAt(0, server.Properties.Folders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Properties.Components.Count());
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.Properties.PleskLicense.Code);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.Properties.PleskLicense.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.Properties.PleskLicense.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.Properties.PleskLicense.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.Properties.PleskLicense.ServerIp);
        mapped?.Properties?.PleskLicense?.Addons.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void Server_Project_Success()
    {
        var server = CreateServer();
        server.Project = null;

        var mapped = server.MapToResponse();
        mapped?.Metadata?.Project.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Server_Version_1_Success_NotNull()
    {
        var server = CreateServer();
        server.Version = null;

        var mapped = server.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Server_Version_2_Success_NotNull()
    {
        var server = CreateServer();
        server.Version = new Abstractions.Models.Version();

        var mapped = server.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Server_Version_3_Success_NotNull()
    {
        var server = CreateServer();
        server.Version = new Abstractions.Models.Version()
        {
            Data = new DataVersion()
        };

        var mapped = server.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Server_Version_4_Success_NotNull()
    {
        var server = CreateServer();
        server.Version = new Abstractions.Models.Version()
        {
            Data = new DataVersion()
            {
                Current = 3
            }
        };

        var mapped = server.MapToResponse();
        mapped?.Metadata?.Version.Should().Be(server.Version.Data.Current.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    [Unit]
    public void Server_Success_Null()
    {
        Server request = null;

        var mapped = request.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Server_Folders_Null_Success()
    {
        var server = CreateServer();
        server.Properties!.Folders = null;
        var mapped = server.MapToResponse();
        mapped.Should().NotBeNull();
        mapped!.Properties!.Folders.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void Server_ListResponse_Success()
    {
        var servers = new ServerList()
        {
            Values = new List<Server>() { CreateServer() },
            TotalCount = 1
        };
        var httpContext = new DefaultHttpContext();
        var mapped = servers.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(servers.TotalCount);
        mapped!.Values.Should().HaveCount((int)servers.TotalCount);
    }

    [Fact]
    [Unit]
    public void ServerIp_ListResponse_Null_Success()
    {
        var ipAddresses = new ServerIpAddressList()
        {
            Values = new List<IpAddress>()
            {
                new IpAddress()
                {
                    Description = "Description1",
                    HostNames = new List<string>(){"HostName1a", "HostName1b" },
                    Id = 1,
                    Ip = "127.0.0.1",
                    Type = IpAddressTypes.Primary
                },
                 new IpAddress()
                {
                    Description = "Description2",
                    HostNames = new List<string>(){"HostName2a", "HostName2b" },
                    Id = 2,
                    Ip = "127.0.0.2",
                    Type = IpAddressTypes.Additional
                },
                new IpAddress()
                {
                    Description = "Description3",
                    HostNames = null,
                    Id = 3,
                    Ip = "127.0.0.3",
                    Type = IpAddressTypes.Management
                }
            },
            TotalCount = 3
        };
        var httpContext = new DefaultHttpContext();
        var mapped = ipAddresses.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalCount);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalCount);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(0, IpAddressTypes.Primary);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(1, IpAddressTypes.Additional);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(2, IpAddressTypes.Management);
        mapped!.Values.Last().HostNames.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void ServerIp_ListResponse_Success()
    {
        var ipAddresses = new ServerIpAddressList()
        {
            Values = new List<IpAddress>()
            {
                new IpAddress()
                {
                    Description = "Description1",
                    HostNames = new List<string>(){"HostName1a", "HostName1b" },
                    Id = 1,
                    Ip = "127.0.0.1",
                    Type = IpAddressTypes.Primary
                },
                 new IpAddress()
                {
                    Description = "Description2",
                    HostNames = new List<string>(){"HostName2a", "HostName2b" },
                    Id = 2,
                    Ip = "127.0.0.2",
                    Type = IpAddressTypes.Additional
                },
                new IpAddress()
                {
                    Description = "Description3",
                    HostNames = null,
                    Id = 3,
                    Ip = "127.0.0.3",
                    Type = IpAddressTypes.Management
                }
            },
            TotalCount = 3
        };
        var httpContext = new DefaultHttpContext();
        var mapped = ipAddresses.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalCount);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalCount);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(0, IpAddressTypes.Primary);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(1, IpAddressTypes.Additional);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(2, IpAddressTypes.Management);
        mapped!.Values.Last().HostNames.Should().HaveCount(0);
    }

    #endregion

    #region Model
    [Fact]
    [Unit]
    public void ServerProperties_WithPlesk_NoAddons_Success()
    {
        var server = CreateLegacyServerDetail(true, false);
        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model",false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Components.Count());
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.LicenseCode);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.IpAddress);
        mapped?.Properties?.PleskLicense?.Addons.Should().HaveCount(0);
        mapped?.Properties?.PleskLicense?.Addons.Should().NotBeNull();
        mapped?.Properties?.OriginalName.Should().Be(server.Name);
    }

    [Fact]
    [Unit]
    public void ServerProperties_WithPlesk_WithAddons_Success()
    {
        var server = CreateLegacyServerDetail(true, true);
        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model",false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Components.Count());
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.LicenseCode);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.IpAddress);
        mapped?.Properties?.OriginalName.Should().Be(server.Name);
        mapped?.Properties?.PleskLicense?.Addons.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Addons.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public void ServerProperties_WithoutPlesk_InvalidLegacyPlesk_Success()
    {
        var server = CreateLegacyServerDetail(true, true);
        foreach (var item in server.PleskLicensesInfo)
        {
            item.isAddon = true;
        }
        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model",false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Components.Count());
        mapped?.Properties?.PleskLicense.Should().BeNull();
    }


    [Fact]
    [Unit]
    public void ServerProperties_WithoutPlesk_Success()
    {
        var server = CreateLegacyServerDetail(false, false);
        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model", false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(server.Components.Count());
        mapped?.Properties?.PleskLicense.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void ServerProperties_WithoutComponents_Success()
    {
        var server = CreateLegacyServerDetail(true, false);
        server.Components = null;
        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model", false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(0);
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.LicenseCode);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.IpAddress);
    }


    [Fact]
    [Unit]
    public void ServerProperties_WithComponents_Null_Success()
    {
        var server = CreateLegacyServerDetail(true, false);
        server.Components = server.Components.Concat(new List<LegacyComponent>() { null });

        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        var mapped = server.MapToDetail("AM", project, location, "server name model", false);
        ValidateLegacyServer(server, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(server.Model);
        mapped?.Properties?.Processor.Should().Be(server.CPU);
        mapped?.Properties?.OperatingSystem.Should().Be(server.OS);
        mapped?.Properties?.Ram.Should().Be(server.RAM);
        mapped?.Properties?.Gpu.Should().Be(server.GPU);
        mapped?.Properties?.Hdd.Should().Be(server.Hdd);
        mapped?.Properties?.ModelTypeCode.Should().Be(server.ModelTypeCode);
        mapped?.Properties?.IpAddress.Should().Be(server.IpAddress);
        mapped?.Properties?.DueDate.Should().Be(server.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(server.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(server.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(server.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(server.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(server.UpgradeAllowed);
        mapped?.Properties?.RenewUpgradeAllowed.Should().Be(server.RenewUpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(server.IncludedInFolders.Count()).And.HaveElementAt(0, server.IncludedInFolders.FirstOrDefault());
        mapped?.Properties?.Components.Should().NotBeNull().And.HaveCount(1);
        mapped?.Properties?.PleskLicense.Should().NotBeNull();
        mapped?.Properties?.PleskLicense?.Code.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.LicenseCode);
        mapped?.Properties?.PleskLicense?.ActivationCode.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationCode);
        mapped?.Properties?.PleskLicense?.ActivationDate.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.ActivationDate);
        mapped?.Properties?.PleskLicense?.Description.Should().Be(server.PleskLicensesInfo.FirstOrDefault()?.Description);
        mapped?.Properties?.PleskLicense?.ServerIp.Should().Be(server.IpAddress);
        mapped?.Properties?.OriginalName.Should().Be(server.Name);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>() { LegacyCatalogItem };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_CategoryOfferte_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>() {
            new LegacyCatalogItem()
            {
                Category = "OFFERTE",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName =  "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-CPU",ProductName = "a"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="b-HDD",ProductName = "b"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-HW",ProductName = "ProductName"},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().NotBeNull();
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be("a");
        mapped.First().Connectivity.Should().Be("1 GB/s");
        mapped.First().Hdd.Should().Be("b");
        mapped.First().Ram.Should().BeNull();
        mapped.First().ServerName.Should().Be("ProductName");
    }


    [Fact]
    [Unit]
    public void ServerCatalog_CategoryOfferte_ProductCode_Null_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "OFFERTE",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode=null},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().NotBeNull();
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().BeNull();
        mapped.First().Connectivity.Should().Be("1 GB/s");
        mapped.First().Hdd.Should().BeNull();
        mapped.First().Ram.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void ServerCatalog_InvalidConfigProducts_Success()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-CPaU"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-HDaD"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-RAaM"},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_ConfigProducts_ProductCode_Null_Success()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode=null},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_WithConfigs_Null_Success()
    {
        var legacyCatalog = new List<LegacyCatalogItem>()
        {
            new LegacyCatalogItem()
            {
                Category = "OFFERTE",
                DiscountedPrice = 1,
                IsSoldOut = true,
                Price = 2,
                ProductCode = "ProductCode",
                SetupFeePrice = 3,
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = null
            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR - Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().BeNull();
        mapped.First().Connectivity.Should().Be("1 GB/s");
        mapped.First().Hdd.Should().BeNull();
        mapped.First().Ram.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void ServerCatalog_WithConfigs_Empty_Success()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }

    [Fact]
    [Unit]
    public void ServerCatalog_CategoryOfferte_ServerNames_Empty()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode=null},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog>();

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().BeNull();
        mapped.First().Connectivity.Should().Be("1 Gb/s");
        mapped.First().Hdd.Should().BeNull();
        mapped.First().Ram.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void ServerCatalog_CategoryOfferte_ServerNames_Invalid()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode=null},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }



    [Fact]
    [Unit]
    public void ServerCatalog_CategoryOfferte_ServerNames_Name_Null()
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
                DisplayName = "Dell PowerEdge R240",
                BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode=null},
                },

            }
        };

        var serverNameMaps = new List<InternalServerCatalog> { ServerCatalogMock };

        var mapped = legacyCatalog.MapToServerCatalog(serverNameMaps, "it");
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be(ServerCatalogMock.Location);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Processor.Should().Be(ServerCatalogMock.Data.First().Cpu);
        mapped.First().Connectivity.Should().Be(ServerCatalogMock.Data.First().Connectivity);
        mapped.First().Hdd.Should().Be(ServerCatalogMock.Data.First().Hdd);
        mapped.First().Ram.Should().Be(ServerCatalogMock.Data.First().Ram);
    }

    [Fact]
    [Unit]
    public void ListLegacyIpAddress_Success()
    {
        var legacyIpAddresses = new LegacyListResponse<LegacyIpAddress>()
        {
            TotalItems = 1,
            Items = new List<LegacyIpAddress>{
                 new LegacyIpAddress()
                 {
                     IpAddressId = 123,
                     CustomName = "mycustomname",
                     Hosts = new List<string>{ "host1"},
                     IpAddress = "123.123",
                     Status = LegacyIpAddressStatuses.Active,
                     IpType = 1
                 }
            }
        };

        var mapped = legacyIpAddresses.Map();
        mapped.Should().NotBeNull();
        mapped.Values.Should().HaveCount(1);
        mapped.Values.First().Id.Should().Be(123);
        mapped.Values.First().Ip.Should().Be("123.123");
        mapped.Values.First().Type.Should().Be(IpAddressTypes.Management);
        mapped.Values.First().Status.Should().Be(IpAddressStatuses.Active);
        mapped.Values.First().Description.Should().Be("mycustomname");
        mapped.Values.First().HostNames.First().Should().Be("host1");
    }

    [Fact]
    [Unit]
    public void ServerProperties_ServerMaps_Model_Invalid()
    {
        var serverNameMaps = new List<InternalServerCatalog>()
        {
            new InternalServerCatalog(){ Model = "aaaa"}
        };
        var server = new LegacyServerListItem()
        {
            Model = "model"
        };

        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };

        var mapped = server.MapToListItem("1", project, location, serverNameMaps);
        mapped?.Properties?.ServerName.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void ServerProperties_ServerMaps_Model_Null()
    {
        var serverNameMaps = new List<InternalServerCatalog>()
        {
            new InternalServerCatalog(){ Model = null}
        };
        var server = new LegacyServerListItem()
        {
            Model = "model"
        };

        var project = new Abstractions.Providers.Models.Projects.Project()
        {
            Metadata = new()
            {
                Id = "1",
                Name = "Project",
                CreatedBy = "AM",
                CreationDate = DateTimeOffset.Now,
            }
        };
        var location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };

        var mapped = server.MapToListItem("1", project, location, serverNameMaps);
        mapped?.Properties?.ServerName.Should().BeNullOrWhiteSpace();
    }

    #endregion

    private static Server CreateServer()
    {
        var ret = CommonMappingTests.CreateBaseResource<Server>();
        ret.Properties = new ServerProperties();
        return ret;
    }

    private static LegacyServerDetail CreateLegacyServerDetail(bool hasPleskLicensee, bool hasPleaskAddons)
    {
        var ret = CommonMappingTests.CreateBaseLegacyResource<LegacyServerDetail>(new LegacyComponent() { Name = "c1", Quantity = 1 });
        ret.Status = ServerStatuses.Active;
        ret.Model = "Model";
        ret.CPU = "CPU";
        ret.GPU = "GPU";
        ret.RAM = "RAM";
        ret.OS = "OS";
        ret.IpAddress = "127.0.0.1";
        ret.ServerFarmCode = "Campus Arezzo";
        ret.Hdd = "Disk";
        ret.ModelTypeCode = "Advanced";
        ret.Name = "OriginalName";
        if (hasPleskLicensee)
        {
            var pleskLicense = new List<LegacyServerPleskLicense>()
            {
                new LegacyServerPleskLicense()
                {
                    ActivationCode = "ActivationCode",
                    ActivationDate = DateTime.Now,
                    LicenseCode = "Code",
                    Description = "Description",
                    isAddon = false
                }
            };
            if (hasPleaskAddons)
            {
                pleskLicense.Add(new LegacyServerPleskLicense()
                {
                    ActivationCode = "ActivationCode",
                    ActivationDate = DateTime.Now,
                    LicenseCode = "Code",
                    Description = "Description",
                    isAddon = true
                });
            }
            ret.PleskLicensesInfo = pleskLicense;
        }
        ret.UpgradeAllowed = true;
        ret.RenewUpgradeAllowed = true;
        return ret;
    }

    private static void ValidateLegacyServer(LegacyServerDetail legacyServer, Server server, Location location, Abstractions.Providers.Models.Projects.Project project)
    {
        CommonMappingTests.ValidateBaseLegacyResource(legacyServer, server, server.UpdatedBy!);
        server?.Uri.Should().Be($"/projects/{server!.Project!.Id}/providers/Aruba.Baremetal/servers/{server.Id}");
        server?.Category?.Should().NotBeNull();
        server?.Category?.Name.Should().Be(Abstractions.Constants.Categories.BaremetalServer.Value);
        server?.Category?.Provider.Should().Be("Aruba.Baremetal");
        server?.Category?.Typology.Should().NotBeNull();
        server?.Category?.Typology?.Id.Should().Be(Typologies.Server.Value);
        server?.Category?.Typology?.Name.Should().Be(Typologies.Server.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
        server?.Project?.Id.Should().Be(project.Metadata.Id);
        server?.Location.Should().NotBeNull();
        server?.Location?.Name.Should().Be(location.Name);
        server?.Location?.City.Should().Be(location.City);
        server?.Location?.Code.Should().Be(location.Code);
        server?.Location?.Country.Should().Be(location.Country);
        server?.Location?.Value.Should().Be(location.Value);

    }

    private InternalServerCatalog ServerCatalogMock =>
        new InternalServerCatalog()
        {
            ServerName = "server name model",
            Model = "Dell PowerEdge R240",
            Location = "ITAR - Arezzo",
            ProductCode = "ProductCode",
            Data = new List<InternalServerCatalogData>()
            {
                new InternalServerCatalogData
                {
                    Connectivity = "1 GB/s",
                    Cpu = "Intel Xeon E-2234G o sup. 4 Core (4x 3.5 GHz)",
                    Ram = "32 GB (DDR4 - ECC)",
                    Gpu = null,
                    Hdd = "2x 480 GB SSD SATA",
                    Language = "it"
                }
            }
        };

    private LegacyCatalogItem LegacyCatalogItem =>
        new LegacyCatalogItem()
        {
            Category = "Category",
            DiscountedPrice = 1,
            IsSoldOut = true,
            Price = 2,
            ProductCode = "ProductCode",
            SetupFeePrice = 3,
            DisplayName = "Dell PowerEdge R240",
            BaseConfigProducts = new List<LegacyCatalogItemConfigProduct>()
                {
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-CPU"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-HDD"},
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-RAM"},
                },

        };
}
