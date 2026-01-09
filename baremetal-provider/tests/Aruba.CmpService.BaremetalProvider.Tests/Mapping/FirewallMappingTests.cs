using System.Collections.ObjectModel;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.CmpService.ResourceProvider.Common.Messages.v1.Enums;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;

public class FirewallMappingTests : TestBase
{
    public FirewallMappingTests(ITestOutputHelper output)
        : base(output)
    { }

    #region SearchFilters
    [Fact]
    [Unit]
    public void SearchFilter_Ok()
    {
        var request = new FirewallIpAddressesFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
        };
        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeTrue();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void SearchFilter_Null()
    {
        var mapped = ((FirewallIpAddressesFilterRequest)null).Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void SearchFilter_QueryNull()
    {
        var request = new FirewallIpAddressesFilterRequest()
        {
            External = false,
            Query = null
        };
        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    #endregion

    #region Response
    [Fact]
    [Unit]
    public void FirewallCatalog_Success_EmptyItems()
    {
        var catalog = new FirewallCatalog();
        var httpContext = new DefaultHttpContext();
        var mapped = catalog.MapToResponse(httpContext.Request);


        mapped.Should().NotBeNull();
        mapped.TotalCount.Should().Be(0);
        mapped.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void FirewallCatalog_Success_WithItems()
    {
        var catalog = new FirewallCatalog()
        {
            TotalCount = 1,
            Values = new List<FirewallCatalogItem>()
            {
                new FirewallCatalogItem()
                {
                    Category = "Category",
                    Name = "Name",
                    Code = "Code",
                    DiscountedPrice = 1,
                    IsSoldOut = true,
                    Location = "Location",
                    Price = 2,
                    SetupFeePrice = 3,
                    Mode = "Mode"
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
        mapped.Values[0].Mode.Should().Be(catalog.Values.First().Mode);
        mapped.Values[0].DiscountedPrice.Should().Be(catalog.Values.First().DiscountedPrice);
        mapped.Values[0].IsSoldOut.Should().Be(catalog.Values.First().IsSoldOut);
        mapped.Values[0].Location.Should().Be(catalog.Values.First().Location);
        mapped.Values[0].Price.Should().Be(catalog.Values.First().Price);
        mapped.Values[0].SetupFeePrice.Should().Be(catalog.Values.First().SetupFeePrice);
    }

    [Fact]
    [Unit]
    public void FirewallCatalog_Success_With1ItemsNULL()
    {
        var catalog = new FirewallCatalog()
        {
            TotalCount = 1,
            Values = new List<FirewallCatalogItem>()
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
    public void FirewallListResponse_To_FirewallListResponseDto()
    {
        var firewallListResponse = new FirewallList()
        {
            TotalCount = 1,
            Values = new List<Firewall>()
            {
                FirewallModel
            }
        };

        var mapped = firewallListResponse.MapToResponse(Substitute.For<HttpRequest>());
        mapped.Should().NotBeNull();
        CommonMappingTests.ValidateBaseResource<Firewall, FirewallResponseDto, FirewallPropertiesResponseDto>(firewallListResponse.Values.First(), mapped.Values.First(), "aru-25085");
    }

    [Fact]
    [Unit]
    public void Firewall_Success()
    {
        var mapped = FirewallModel.MapToResponse();
        mapped.Should().NotBeNull();
        CommonMappingTests.ValidateBaseResource<Firewall, FirewallResponseDto, FirewallPropertiesResponseDto>(FirewallModel, mapped, "aru-25085");
    }

    [Fact]
    [Unit]
    public void Firewall_Null_Success()
    {
        var mapped = ((Firewall?)default).MapToResponse();
        mapped.Should().BeNull();
    }


    [Fact]
    [Unit]
    public void FirewallIp_ListResponse_Success()
    {
        var ipAddresses = new FirewallIpAddressList()
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
                    HostNames = new List<string>(){"HostName2a" },
                    Id = 2,
                    Ip = "127.0.0.2",
                    Type = IpAddressTypes.Management
                }
            },
            TotalCount = 2
        };
        var httpContext = new DefaultHttpContext();
        var mapped = ipAddresses.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalCount);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalCount);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(0, IpAddressTypes.Primary);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(1, IpAddressTypes.Management);
        mapped!.Values.Last().HostNames.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public void FirewallIp_ListResponse_NullHostNames_Success()
    {
        var ipAddresses = new FirewallIpAddressList()
        {
            Values = new List<IpAddress>()
            {
                new IpAddress()
                {
                    Description = "Description1",
                    HostNames = null,
                    Id = 1,
                    Ip = "127.0.0.1",
                    Type = IpAddressTypes.Primary
                },
                 new IpAddress()
                {
                    Description = "Description2",
                    HostNames = new List<string>(){"HostName2a" },
                    Id = 2,
                    Ip = "127.0.0.2",
                    Type = IpAddressTypes.Management
                }
            },
            TotalCount = 2
        };
        var httpContext = new DefaultHttpContext();
        var mapped = ipAddresses.MapToResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalCount);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalCount);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(0, IpAddressTypes.Primary);
        mapped!.Values.Select(s => s.Type).Should().HaveElementAt(1, IpAddressTypes.Management);
        mapped!.Values.Last().HostNames.Should().HaveCount(1);
        mapped!.Values.First().HostNames.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void FirewallVlanid_ListResponse_Success()
    {
        var vlanids = new FirewallVlanIdList()
        {
            Values = new List<VlanId>()
            {
                new VlanId()
                {
                    Vlanid = "123"
                },
                 new VlanId()
                {
                     Vlanid = "abc"
                }
            },
            TotalCount = 2
        };
        var httpContext = new DefaultHttpContext();
        var mapped = vlanids.MapToVlanIdResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(vlanids.TotalCount);
        mapped!.Values.Should().HaveCount((int)vlanids.TotalCount);
        mapped!.Values.First().Vlanid.Should().Be("123");
        mapped!.Values.Last().Vlanid.Should().Be("abc");
    }

    [Fact]
    [Unit]
    public void FirewallVlanid_ListResponse_Empty_Success()
    {
        var vlanids = new FirewallVlanIdList()
        {
            Values = new List<VlanId>(),
            TotalCount = 0
        };
        var httpContext = new DefaultHttpContext();
        var mapped = vlanids.MapToVlanIdResponse(httpContext.Request);
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(vlanids.TotalCount);
        mapped!.Values.Should().HaveCount((int)vlanids.TotalCount);
    }

    #endregion

    #region Model
    [Fact]
    [Unit]
    public void FirewallCatalog_Success()
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
                },

            }
        };
        var mapped = legacyCatalog.MapToFirewallCatalog(FirewallCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().Mode.Should().Be("a");
    }

    [Fact]
    [Unit]
    public void FirewallCatalog_WithConfigs_Null_Success()
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
        var mapped = legacyCatalog.MapToFirewallCatalog(FirewallCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);        
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Location.Should().Be("ITAR-Arezzo");
        mapped.First().Mode.Should().Be("a");
    }

    [Fact]
    [Unit]
    public void FirewallCatalog_WithConfigs_Empty_Success()
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
        var mapped = legacyCatalog.MapToFirewallCatalog(new List<InternalFirewallCatalog>());
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().BeNull();
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Mode.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void FirewallCatalog_ProductCode_Null()
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
                },

            }
        };
        var mapped = legacyCatalog.MapToFirewallCatalog(FirewallCatalogsMock);
        mapped.Should().NotBeNull().And.HaveCount(1);
        mapped.First().IsSoldOut.Should().Be(legacyCatalog[0].IsSoldOut);
        mapped.First().Code.Should().Be(legacyCatalog[0].ProductCode);
        mapped.First().Price.Should().Be(legacyCatalog[0].Price);
        mapped.First().Name.Should().Be(legacyCatalog[0].DisplayName);
        mapped.First().Category.Should().Be(legacyCatalog[0].Category);
        mapped.First().Location.Should().BeNullOrWhiteSpace();
        mapped.First().SetupFeePrice.Should().Be(legacyCatalog[0].SetupFeePrice);
        mapped.First().DiscountedPrice.Should().Be(legacyCatalog[0].DiscountedPrice);
        mapped.First().Mode.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void LegacyFirewallListItem_To_Firewall()
    {
        var legacyFirewallListItem = new LegacyFirewallListItem()
        {
            Id = 123,
            Name = "Name",
            IncludedInFolders = new List<string>() { "folder1" },
            ExpirationDate = new DateTime(2026, 01, 30),
            Status = FirewallStatuses.Active,
            Model = "model",
            ConfigurationMode = "manual",
            ActivationDate = new DateTime(2025, 01, 10),
            ServerFarmCode = Guid.NewGuid().ToString()
        };

        var mapped = legacyFirewallListItem.MapToListitem("aru-25085", GetProject(), GetLocationModel());
        mapped.Should().NotBeNull();
        mapped.Id.Should().Be(legacyFirewallListItem.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Name.Should().Be(legacyFirewallListItem.Name);
        mapped.Status.State.Should().Be(legacyFirewallListItem.Status.ToString());
        mapped.Properties.Folders.Should().HaveCount(1);
        mapped.Properties.Folders.FirstOrDefault().Should().Be(legacyFirewallListItem.IncludedInFolders.First());
        mapped.Properties.DueDate.Should().Be(legacyFirewallListItem.ExpirationDate);
        mapped.Properties.ActivationDate.Should().Be(legacyFirewallListItem.ActivationDate);
        mapped.Properties.Model.Should().Be(legacyFirewallListItem.Model);
        mapped.Properties.ConfigurationMode.Should().Be(legacyFirewallListItem.ConfigurationMode);
        mapped.Location.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void LegacyFirewallDetail_To_Firewall()
    {
        var legacyFirewallDetail = new LegacyFirewallDetail()
        {
            Status = FirewallStatuses.Active,
            Model = "model",
            CustomName = "admin",
            IpAddress = "127.0.0.1",
            ConfigurationMode = "manual",
            ServerFarmCode = Guid.NewGuid().ToString(),
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

        var mapped = legacyFirewallDetail.MapToDetail("aru-25085", GetProject(), GetLocationModel(), false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacyFirewallDetail.ActivationDate);
        mapped.Name.Should().Be(legacyFirewallDetail.CustomName);
        mapped.Location!.Value.Should().Be(GetLocationModel().Value);
        mapped.Id.Should().Be(legacyFirewallDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version!.Data!.Current.Should().Be(1);
        mapped.Uri.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void LegacyFirewallDetail_To_Firewall_NoCustomName()
    {
        var legacyFirewallDetail = new LegacyFirewallDetail()
        {
            Status = FirewallStatuses.Active,
            Model = "model",
            IpAddress = "127.0.0.1",
            ConfigurationMode = "manual",
            ServerFarmCode = Guid.NewGuid().ToString(),
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

        var mapped = legacyFirewallDetail.MapToDetail("aru-25085", GetProject(), GetLocationModel(),false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacyFirewallDetail.ActivationDate);
        mapped.Name.Should().Be(legacyFirewallDetail.Name);
        mapped.Location.Value.Should().Be(GetLocationModel().Value);
        mapped.Id.Should().Be(legacyFirewallDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version.Data.Current.Should().Be(1);
        mapped.Uri.Should().NotBeNull();
    }


    [Fact]
    [Unit]
    public void LegacyFirewallDetail_To_Firewall_IncludedInFolder_null()
    {
        var legacyFirewallDetail = new LegacyFirewallDetail()
        {
            Status = FirewallStatuses.Active,
            Model = "model",
            IpAddress = "127.0.0.1",
            ConfigurationMode = "manual",
            ServerFarmCode = Guid.NewGuid().ToString(),
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

        var mapped = legacyFirewallDetail.MapToDetail("aru-25085", GetProject(), GetLocationModel(),false);
        mapped.Should().NotBeNull();
        mapped.CreatedBy.Should().Be("aru-25085");
        mapped.CreationDate.Should().Be(legacyFirewallDetail.ActivationDate);
        mapped.Name.Should().Be(legacyFirewallDetail.Name);
        mapped.Location.Value.Should().Be(GetLocationModel().Value);
        mapped.Id.Should().Be(legacyFirewallDetail.Id.ToString(CultureInfo.InvariantCulture));
        mapped.Category.Should().NotBeNull();
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Version.Data.Current.Should().Be(1);
        mapped.Properties.Folders.Should().HaveCount(0);
        mapped.Uri.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void FirewallIpAddress_ListResponse_Items_Empty()
    {
        var ipAddresses = new LegacyListResponse<LegacyIpAddress>();
        var mapped = ipAddresses.MapToFirewallIpAddressList();
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalItems);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalItems);
    }

    [Fact]
    [Unit]
    public void FirewallIpAddress_ListResponse_Items_Null()
    {
        var ipAddresses = new LegacyListResponse<LegacyIpAddress>()
        {
            Items = null
        };
        var mapped = ipAddresses.MapToFirewallIpAddressList();
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(ipAddresses.TotalItems);
        mapped!.Values.Should().HaveCount((int)ipAddresses.TotalItems);
    }

    [Fact]
    [Unit]
    public void Firewall_MapToBaremetalResource_Success()
    {
        var firewall = new Firewall()
        {
            Id = "101",
            Uri = "https://example.com/firewall/101",
            Name = "Test-Firewall",
            Location = new Location()
            {
                City = "Bergamo",
                Name = "IT1",
                Code = "IT-BG",
                Country = "Italy",
                Value = "BG"
            },
            Project = new Project()
            {
                Id = "project-123"
            },
            Tags = new Collection<string> { "tag1", "tag2" },
            CreationDate = DateTimeOffset.Now.AddMonths(-1),
            UpdateDate = DateTimeOffset.Now,
            CreatedBy = "admin",
            UpdatedBy = "editor",
            LinkedResources = new Collection<LinkedResource>()
            {
                new LinkedResource()
                {
                    Id = "201",
                    LinkCreationDate = DateTimeOffset.Now.AddDays(-10),
                    LinkType = LinkedResourceType.Use,
                    Ready = true,
                    RelationName = "linked-vm",
                    StrictCorrelation = false,
                    Typology = "vm",
                    Uri = "https://example.com/vm/201"
                }
            },
            Status = new Status()
            {
                State = "Disabled",
                CreationDate = DateTimeOffset.Now.AddMonths(-1),
                DisableStatusInfo = new DisableStatusInfo()
                {
                    IsDisabled = true,
                    Reasons = new Collection<DisableReasonDetail>()
                    {
                        new DisableReasonDetail()
                        {                            
                            CreationDate = DateTimeOffset.Now.AddDays(-5),
                            Mode = Abstractions.Models.DisableMode.Automatic,
                            Note = "Auto-disabled ",
                            Reason = "NoCredit"
                        }
                    },
                    PreviousStatus = new PreviousStatus()
                    {
                        CreationDate = DateTimeOffset.Now.AddMonths(-2),
                        State = "Active"
                    }
                }
            },
            Category = new Category()
            {
                Name = "Network",
                Typology = new Typology()
                {
                    Id = "firewall",
                    Name = "Firewall"
                }
            },
            Version = new Abstractions.Models.Version()
            {
                Data = new DataVersion()
                {
                    Current = 3,
                    Previous = 2
                }
            }
        };

        var action = AutorenewFolderAction.DisableSafeFolder;

        var mapped = firewall.Map(action);

        mapped.Should().NotBeNull();
        mapped.Action.Should().Be(action);
        mapped.Id.Should().Be(firewall.Id);
        mapped.Uri.Should().Be(firewall.Uri);
        mapped.Name.Should().Be(firewall.Name);

        mapped.Location.Should().NotBeNull();
        mapped.Location.City.Should().Be("Bergamo");
        mapped.Location.Name.Should().Be("IT1");
        mapped.Location.Code.Should().Be("IT-BG");
        mapped.Location.Country.Should().Be("Italy");
        mapped.Location.Value.Should().Be("BG");

        mapped.Project.Should().NotBeNull();
        mapped.Project.Id.Should().Be("project-123");

        mapped.Tags.Should().BeEquivalentTo(firewall.Tags);
        mapped.CreationDate.Should().Be(firewall.CreationDate);
        mapped.UpdateDate.Should().Be(firewall.UpdateDate);
        mapped.CreatedBy.Should().Be(firewall.CreatedBy);
        mapped.UpdatedBy.Should().Be(firewall.UpdatedBy);

        mapped.LinkedResources.Should().HaveCount(1);
        var linked = mapped.LinkedResources.First();
        linked.Id.Should().Be("201");
        linked.LinkType.Should().Be(LinkedResourceType.Use);
        linked.Ready.Should().BeTrue();
        linked.RelationName.Should().Be("linked-vm");
        linked.StrictCorrelation.Should().BeFalse();
        linked.Typology.Should().Be("vm");
        linked.Uri.Should().Be("https://example.com/vm/201");

        mapped.Status.Should().NotBeNull();
        mapped.Status.State.Should().Be("Disabled");
        mapped.Status.CreationDate.Should().Be(firewall.Status.CreationDate);
        mapped.Status.DisableStatusInfo.Should().NotBeNull();
        mapped.Status.DisableStatusInfo.IsDisabled.Should().BeTrue();
        mapped.Status.DisableStatusInfo.Reasons.Should().ContainSingle().And.Contain("NoCredit");
        mapped.Status.DisableStatusInfo.ReasonDetails.Should().ContainSingle();
        mapped.Status.DisableStatusInfo.ReasonDetails.First().Mode.Should().Be(ResourceProvider.Common.Messages.v1.Enums.DisableMode.Automatic);
        mapped.Status.DisableStatusInfo.PreviousStatus.Should().NotBeNull();
        mapped.Status.DisableStatusInfo.PreviousStatus.State.Should().Be("Active");

        mapped.Category.Should().NotBeNull();
        mapped.Category.Name.Should().Be("Network");
        mapped.Category.Typology.Should().NotBeNull();
        mapped.Category.Typology.Id.Should().Be("firewall");
        mapped.Category.Typology.Name.Should().Be("Firewall");

        mapped.Version.Should().NotBeNull();
        mapped.Version.Data.Current.Should().Be(3);
        mapped.Version.Data.Previous.Should().Be(2);
    }


    [Fact]
    [Unit]
    public void FirewallVlanId_ListResponse_Items_Empty()
    {
        var vlanids = new LegacyListResponse<LegacyVlanId>();
        var mapped = vlanids.MapToFirewallVlanIdList();
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(vlanids.TotalItems);
        mapped!.Values.Should().HaveCount((int)vlanids.TotalItems);
    }

    [Fact]
    [Unit]
    public void FirewallVlanid_ListResponse_Items_Null()
    {
        var vlanids = new LegacyListResponse<LegacyVlanId>()
        {
            Items = null
        };
        var mapped = vlanids.MapToFirewallVlanIdList();
        mapped!.Should().NotBeNull();
        mapped!.TotalCount.Should().Be(vlanids.TotalItems);
        mapped!.Values.Should().HaveCount((int)vlanids.TotalItems);
    }
    #endregion

    #region Utils

    private static Firewall FirewallModel =
                 new Firewall()
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
                     Properties = GetFirewallPropertiesModel(),
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

    private static FirewallProperties GetFirewallPropertiesModel() => new FirewallProperties()
    {
        Admin = "admin",
        RenewAllowed = true,
        ActivationDate = DateTimeOffset.UtcNow,
        AutoRenewEnabled = true,
        DueDate = DateTimeOffset.UtcNow,
        Model = "model",
        IpAddress = "123.123.123",
        ConfigurationMode = "manual",
        MonthlyUnitPrice = 50.00m
    };

    private List<InternalFirewallCatalog> FirewallCatalogsMock =>
        new List<InternalFirewallCatalog> {
            new InternalFirewallCatalog()
            {
                Code = "ProductCode",
                Location = "ITAR-Arezzo",
                Mode = "a"

            }
        };
    #endregion
}
