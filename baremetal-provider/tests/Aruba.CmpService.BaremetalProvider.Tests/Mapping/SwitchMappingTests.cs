using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;
public class SwitchMappingTests : TestBase
{
    public SwitchMappingTests(ITestOutputHelper output)
        : base(output)
    {
    }

    #region Response
    [Fact]
    [Unit]
    public void Switch_Success_Baremetal_Resource_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Status = new Status()
        {
            State = "Disabled",
            DisableStatusInfo = new DisableStatusInfo()
            {
                Reasons = new System.Collections.ObjectModel.Collection<DisableReasonDetail>()
                {
                    new DisableReasonDetail()
                    {
                        CreationDate = DateTime.UtcNow,
                        Mode = DisableMode.Automatic,
                        Note = "no notes",
                        Reason = "NoCredit"
                    }
                }
            }
        };
        @switch.LinkedResources = new System.Collections.ObjectModel.Collection<LinkedResource>()
        {
            new LinkedResource()
            {
                Id = "123",
                Uri = "uri/123",
                LinkType = ResourceProvider.Common.Dtos.Shared.LinkedResourceType.Use,
                Ready = true,
                RelationName = "relationName",
                StrictCorrelation = true,
                LinkCreationDate = DateTime.UtcNow,
                Typology = "server",
            }
        };
        @switch.Properties = new SwitchProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            AutoRenewEnabled = true,
            DueDate = DateTimeOffset.Now,
            Folders = new List<string>() { "Folder1" },
            Model = "Model",
            MonthlyUnitPrice = 2,
            RenewAllowed = false,
            Admin = "Admin",
        };
        @switch.UpdatedBy = "AM";

        var mapped = @switch.Map(AutorenewFolderAction.DisableSafeFolder);
        CommonMappingTests.ValidateBaseBaremetalResource<Switch>(@switch, mapped!, @switch.UpdatedBy);
        mapped?.Status.DisableStatusInfo.ReasonDetails?.Should().NotBeNull();
        mapped?.LinkedResources?.Should().HaveCount(1);
    }


    [Fact]
    [Unit]
    public void Switch_Success_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Properties = new SwitchProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            AutoRenewEnabled = true,
            DueDate = DateTimeOffset.Now,
            Folders = new List<string>() { "Folder1" },
            Model = "Model",
            MonthlyUnitPrice = 2,
            RenewAllowed = false,
            Admin = "Admin"
        };
        @switch.UpdatedBy = "AM";

        var mapped = @switch.MapToResponse();
        CommonMappingTests.ValidateBaseResource<Switch, SwitchResponseDto, SwitchPropertiesResponseDto>(@switch, mapped!, @switch.UpdatedBy);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.ActivationDate.Should().Be(@switch.Properties.ActivationDate);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(@switch.Properties.AutoRenewEnabled);
        mapped?.Properties?.DueDate.Should().Be(@switch.Properties.DueDate);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(@switch.Properties.Folders.Count()).And.HaveElementAt(0, @switch.Properties.Folders.FirstOrDefault());
        mapped?.Properties?.Model.Should().Be(@switch.Properties.Model);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(@switch.Properties.MonthlyUnitPrice);
        mapped?.Properties?.RenewAllowed.Should().Be(@switch.Properties.RenewAllowed);
        mapped?.Properties?.Admin.Should().Be(@switch.Properties.Admin);
    }

    [Fact]
    [Unit]
    public void Switch_Project_Success()
    {
        var @switch = CreateSwitch();
        @switch.Project = null;

        var mapped = @switch.MapToResponse();
        mapped?.Metadata?.Project.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Switch_Version_1_Success_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Version = null;

        var mapped = @switch.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Switch_Version_2_Success_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Version = new Abstractions.Models.Version();

        var mapped = @switch.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Switch_Version_3_Success_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Version = new Abstractions.Models.Version()
        {
            Data = new DataVersion()
        };

        var mapped = @switch.MapToResponse();
        mapped?.Metadata?.Version.Should().Be("1");
    }

    [Fact]
    [Unit]
    public void Switch_Version_4_Success_NotNull()
    {
        var @switch = CreateSwitch();
        @switch.Version = new Abstractions.Models.Version()
        {
            Data = new DataVersion()
            {
                Current = 3
            }
        };

        var mapped = @switch.MapToResponse();
        mapped?.Metadata?.Version.Should().Be(@switch.Version.Data.Current.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    [Unit]
    public void Switch_Success_Null()
    {
        Switch request = null;

        var mapped = request.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Switch_Folders_Null_Success()
    {
        var @switch = CreateSwitch();
        @switch.Properties!.Folders = null;
        var mapped = @switch.MapToResponse();
        mapped.Should().NotBeNull();
        mapped!.Properties!.Folders.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void Switch_ListResponse_Success()
    {
        var switches = new SwitchList()
        {
            Values = new List<Switch>() { CreateSwitch() },
            TotalCount = 1
        };
        var httpContext = new DefaultHttpContext();
        var mapped = switches.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(switches.TotalCount);
        mapped!.Values.Should().HaveCount((int)switches.TotalCount);
    }
    [Fact]
    [Unit]
    public void SwitchCatalog_Success_EmptyItems()
    {
        var catalog = new SwitchCatalog();
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);


        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(0);
        mapped.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_Success_WithItems()
    {
        var catalog = new SwitchCatalog()
        {
            TotalCount = 1,
            Values = new List<SwitchCatalogItem>()
            {
                new SwitchCatalogItem()
                {
                    Category = "Category",
                    Name = "Name",
                    Code = "Code",
                    DiscountedPrice = 1,
                    IsSoldOut = true,
                    Location = "Location",
                    Price = 2,
                    SetupFeePrice = 3,
                    Ports = "Ports"
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
        mapped.Values[0].Ports.Should().Be(catalog.Values.First().Ports);
        mapped.Values[0].DiscountedPrice.Should().Be(catalog.Values.First().DiscountedPrice);
        mapped.Values[0].IsSoldOut.Should().Be(catalog.Values.First().IsSoldOut);
        mapped.Values[0].Location.Should().Be(catalog.Values.First().Location);
        mapped.Values[0].Price.Should().Be(catalog.Values.First().Price);
        mapped.Values[0].SetupFeePrice.Should().Be(catalog.Values.First().SetupFeePrice);
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_Success_With1ItemsNULL()
    {
        var catalog = new SwitchCatalog()
        {
            TotalCount = 1,
            Values = new List<SwitchCatalogItem>()
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
    #endregion

    #region Model
    [Fact]
    [Unit]
    public void SwitchProperties_Success()
    {
        var @switch = CreateLegacySwitchDetail();
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
        var mapped = @switch.MapToDetail("AM", project, location, false);
        ValidateLegacySwitchDetail(@switch, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(@switch.Model);
        mapped?.Properties?.Admin.Should().Be(@switch.Name);
        mapped?.Properties?.DueDate.Should().Be(@switch.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(@switch.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(@switch.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(@switch.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(@switch.RenewAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(@switch.IncludedInFolders.Count()).And.HaveElementAt(0, @switch.IncludedInFolders.FirstOrDefault());
    }

    [Fact]
    [Unit]
    public void SwitchListitem_Success()
    {
        var @switch = CreateLegacySwitchListitem();
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
        var mapped = @switch.MapToListitem("AM", project, location);
        ValidateLegacySwitchListitem(@switch, mapped, location, project);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.Model.Should().Be(@switch.Model);
        mapped?.Properties?.DueDate.Should().Be(@switch.ExpirationDate);
        mapped?.Properties?.ActivationDate.Should().Be(@switch.ActivationDate);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(@switch.IncludedInFolders.Count()).And.HaveElementAt(0, @switch.IncludedInFolders.FirstOrDefault());
    }
    public void SwitchCatalog_Success()
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
                    new LegacyCatalogItemConfigProduct(){ProductCode="a-Ports"},
                },

            }
        };
        var mapped = legacyCatalog.MapToSwitchCatalog(SwitchesCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Ports.Should().Be("a-Ports");
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_WithConfigs_Null_Success()
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
                BaseConfigProducts = null
            }
        };
        var mapped = legacyCatalog.MapToSwitchCatalog(SwitchesCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Ports.Should().Be("a");
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_WithConfigs_Empty_Success()
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
            }
        };
        var mapped = legacyCatalog.MapToSwitchCatalog(SwitchesCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Ports.Should().Be("a");
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_WithConfigs_ProductCode_null()
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
                    new LegacyCatalogItemConfigProduct()
                    {
                         ProductCode = null,
                    }
                }

            }
        };
        var mapped = legacyCatalog.MapToSwitchCatalog(SwitchesCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Ports.Should().Be("a");
    }

    [Fact]
    [Unit]
    public void SwitchCatalog_WithConfigs_ProductCode_Invalid()
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
                    new LegacyCatalogItemConfigProduct()
                    {
                         ProductCode = "dkasldklaskdlaskdòls",
                    }
                }

            }
        };
        var mapped = legacyCatalog.MapToSwitchCatalog(SwitchesCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Ports.Should().Be("a");
    }
    #endregion

    private static Switch CreateSwitch()
    {
        var ret = CommonMappingTests.CreateBaseResource<Switch>();
        ret.Properties = new SwitchProperties();
        return ret;
    }

    private static LegacySwitchDetail CreateLegacySwitchDetail()
    {
        var ret = CommonMappingTests.CreateBaseLegacyResource<LegacySwitchDetail>();
        ret.Status = SwitchStatuses.Active;
        ret.Model = "Model";
        ret.ServerFarmCode = "Campus Arezzo";
        ret.Name = "Admin";
        return ret;
    }
    private static LegacySwitchListItem CreateLegacySwitchListitem()
    {
        var ret = CommonMappingTests.CreateBaseLegacyResourceListitem<LegacySwitchListItem>();
        ret.Status = SwitchStatuses.Active;
        ret.Model = "Model";
        ret.ServerFarmCode = "Campus Arezzo";
        ret.Name = "Admin";
        ret.ActivationDate = DateTime.UtcNow;
        return ret;
    }

    private static void ValidateLegacySwitchDetail(LegacySwitchDetail legacySwitch, Switch @switch, Location location, Abstractions.Providers.Models.Projects.Project project)
    {
        CommonMappingTests.ValidateBaseLegacyResource(legacySwitch, @switch, @switch.UpdatedBy!);
        @switch?.Uri.Should().Be($"/projects/{@switch!.Project!.Id}/providers/Aruba.Baremetal/switches/{@switch.Id}");
        @switch?.Category?.Should().NotBeNull();
        @switch?.Category?.Name.Should().Be(Abstractions.Constants.Categories.BaremetalNetwork.Value);
        @switch?.Category?.Provider.Should().Be("Aruba.Baremetal");
        @switch?.Category?.Typology.Should().NotBeNull();
        @switch?.Category?.Typology?.Id.Should().Be(Typologies.Switch.Value);
        @switch?.Category?.Typology?.Name.Should().Be(Typologies.Switch.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
        @switch?.Project?.Id.Should().Be(project.Metadata.Id);
        @switch?.Location.Should().NotBeNull();
        @switch?.Location?.Name.Should().Be(location.Name);
        @switch?.Location?.City.Should().Be(location.City);
        @switch?.Location?.Code.Should().Be(location.Code);
        @switch?.Location?.Country.Should().Be(location.Country);
        @switch?.Location?.Value.Should().Be(location.Value);
    }

    private static void ValidateLegacySwitchListitem(LegacySwitchListItem legacySwitch, Switch @switch, Location location, Abstractions.Providers.Models.Projects.Project project)
    {
        CommonMappingTests.ValidateBaseLegacyResourceListitem(legacySwitch, @switch, @switch.UpdatedBy!);
        @switch?.Uri.Should().Be($"/projects/{@switch!.Project!.Id}/providers/Aruba.Baremetal/switches/{@switch.Id}");
        @switch?.Category?.Should().NotBeNull();
        @switch?.Category?.Name.Should().Be(Abstractions.Constants.Categories.BaremetalNetwork.Value);
        @switch?.Category?.Provider.Should().Be("Aruba.Baremetal");
        @switch?.Category?.Typology.Should().NotBeNull();
        @switch?.Category?.Typology?.Id.Should().Be(Typologies.Switch.Value);
        @switch?.Category?.Typology?.Name.Should().Be(Typologies.Switch.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
        @switch?.Project?.Id.Should().Be(project.Metadata.Id);
        @switch?.Location.Should().NotBeNull();
        @switch?.Location?.Name.Should().Be(location.Name);
        @switch?.Location?.City.Should().Be(location.City);
        @switch?.Location?.Code.Should().Be(location.Code);
        @switch?.Location?.Country.Should().Be(location.Country);
        @switch?.Location?.Value.Should().Be(location.Value);
    }

    private List<InternalSwitchCatalog> SwitchesCatalogsMock =>
    new List<InternalSwitchCatalog> {
            new InternalSwitchCatalog()
            {
                Code = "ProductCode",
                Location = "ITAR-Arezzo",
                Ports = "a",
            }
    };
}
