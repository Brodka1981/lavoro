using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Aruba.MessageBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class FirewallQueryHandlersTests : TestBase
{
    public FirewallQueryHandlersTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<FirewallGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var logger2 = Substitute.For<ILogger<FirewallSearchCatalogQueryHandler>>();
        services.AddSingleton(logger2);

        var firewallsService = Substitute.For<IFirewallsService>();
        services.AddSingleton(firewallsService);

        var projectProvider = Substitute.For<IProjectProvider>();
        services.AddSingleton(projectProvider);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        services.AddSingleton<FirewallGetByIdQueryHandler>();
        services.AddSingleton<FirewallSearchQueryHandler>();
        services.AddSingleton<FirewallSearchCatalogQueryHandler>();
        services.AddSingleton<FirewallSearchIpAddressesQueryHandler>();
        services.AddSingleton<FirewallGetVlanIDsQueryHandler>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetFirewallById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.GetById(new FirewallByIdRequest() { ResourceId = "123" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<Firewall>()
            {
                Value = new Firewall
                {
                    Id = "123",
                    CreatedBy = "aru-24468",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new FirewallProperties()
                }
            });

        var queryHandler = provider.GetRequiredService<FirewallGetByIdQueryHandler>();
        var request = new FirewallByIdRequest()
        {
            ResourceId = "123"
        };

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetFirewallById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.GetById(It.IsAny<FirewallByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<Firewall>.CreateNotFound("1"));


        var queryHandler = provider.GetRequiredService<FirewallGetByIdQueryHandler>();
        var request = new FirewallByIdRequest()
        {
            ResourceId = "121"
        };

        var response = await queryHandler.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.Search(It.IsAny<FirewallSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallList>()
            {
                Value = new FirewallList
                {
                    Values = new List<Firewall>()
                    {
                        new Firewall()
                        {
                           Id = "123",
                           CreatedBy = "aru-24468",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new FirewallProperties()
                        }
                    }
                }
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Count.Should().Be(1);
        firewallResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ProjectResponse_Error()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IFirewallsService), typeof(FirewallsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFirewallsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IFirewallCatalogRepository>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>() { Success = false });


        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ProjectResponse_Result_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IFirewallsService), typeof(FirewallsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFirewallsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IFirewallCatalogRepository>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true
            });


        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ProjectResponse_Result_Properties_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IFirewallsService), typeof(FirewallsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFirewallsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IFirewallCatalogRepository>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
            });


        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ProjectResponse_Result_NonDefault()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IFirewallsService), typeof(FirewallsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IFirewallsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IFirewallCatalogRepository>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
                {
                    Properties = new Abstractions.Providers.Models.Projects.ProjectProperties()
                }
            });


        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ProjectResponse_Result_Default()
    {
        var firewallsProvider = Substitute.For<IFirewallsProvider>();
        firewallsProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>()
            {
                Success = true,
                Result = new LegacyListResponse<LegacyFirewallListItem>()
            });


        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IFirewallsService), typeof(FirewallsService), ServiceLifetime.Singleton))
            .AddSingleton(firewallsProvider)
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IFirewallCatalogRepository>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
                {
                    Properties = new Abstractions.Providers.Models.Projects.ProjectProperties()
                    {
                        Default = true
                    }
                }
            });



        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Should().NotBeNull();
        firewallResponse.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.Search(It.IsAny<FirewallSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallList>.CreateNotFound("a"));

        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.Search(It.IsAny<FirewallSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallList>.CreateForbiddenError());

        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.Search(It.IsAny<FirewallSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallList>.CreateInternalServerError());

        var queryHandler = provider.GetRequiredService<FirewallSearchQueryHandler>();
        var request = new FirewallSearchFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = new FirewallCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest { Query=new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions())};

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = GetCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest 
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
        firewallResponse.TotalCount.Should().Be(200);
        firewallResponse.Values.Should().HaveCount(200);
        firewallResponse.Values.First().Name.Should().Be("Product 00 00");
        firewallResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = GetCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Descending) }
            }
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
        firewallResponse.TotalCount.Should().Be(200);
        firewallResponse.Values.Should().HaveCount(200);
        firewallResponse.Values.First().Name.Should().Be("Product 09 19");
        firewallResponse.Values.Last().Name.Should().Be("Product 00 00");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = GetCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
        firewallResponse.TotalCount.Should().Be(200);
        firewallResponse.Values.Should().HaveCount(200);
        firewallResponse.Values.First().Price.Should().Be(0);
        firewallResponse.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = GetCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
        firewallResponse.TotalCount.Should().Be(200);
        firewallResponse.Values.Should().HaveCount(200);
        firewallResponse.Values.First().Price.Should().Be(10 * 9 * 19);
        firewallResponse.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<FirewallCatalog>()
            {
                Value = GetCatalog()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Filters = new FilterDefinitionsList()
                            {
                                FilterDefinition.Create("fulltextsearch","eq","Product 00 11")
                            }

            }
        };

        var firewallResponse = await queryHandler.Handle(request);
        firewallResponse.Values.Should().NotBeNull();
        firewallResponse.TotalCount.Should().Be(1);
        firewallResponse.Values.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallCatalog>.CreateNotFound("a"));

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallCatalog>.CreateForbiddenError());

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_Catalog_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var firewallsService = provider.GetRequiredService<IFirewallsService>();

        firewallsService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<FirewallCatalog>.CreateInternalServerError());

        var queryHandler = provider.GetRequiredService<FirewallSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchIpAddress_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.SearchIpAddresses(It.IsAny<FirewallIpAddressesFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<FirewallIpAddressList>()
            {
                Value = new FirewallIpAddressList()
            });

        var queryHandler = provider.GetRequiredService<FirewallSearchIpAddressesQueryHandler>();
        var request = new FirewallIpAddressesFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();
        response.Values.Count.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchIpAddress_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.SearchIpAddresses(It.IsAny<FirewallIpAddressesFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(ServiceResult<FirewallIpAddressList>.CreateNotFound(1));

        var queryHandler = provider.GetRequiredService<FirewallSearchIpAddressesQueryHandler>();
        var request = new FirewallIpAddressesFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().BeNull();
    }

    private FirewallCatalog GetCatalog()
    {
        var elements = new List<FirewallCatalogItem>();

        for (var i = 0; i < 10; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                var element = new FirewallCatalogItem();
                element.Code = $"ProductCode_{i}_{x}";
                element.Name = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                element.DiscountedPrice = 20 * i * x;
                element.SetupFeePrice = 30 * i * x;
                elements.Add(element);
            }
        }
        var ret = new FirewallCatalog()
        {
            TotalCount = 200,
            Values = elements,

        };
        return ret;
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetVlanIds_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.GetVlanIds(It.IsAny<FirewallVlanIdFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<FirewallVlanIdList>()
            {
                Value = new FirewallVlanIdList()
            });

        var queryHandler = provider.GetRequiredService<FirewallGetVlanIDsQueryHandler>();
        var request = new FirewallVlanIdFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();
        response.Values.Count.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetVlanIds_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallsService = provider.GetRequiredService<IFirewallsService>();
        firewallsService.GetVlanIds(It.IsAny<FirewallVlanIdFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(ServiceResult<FirewallVlanIdList>.CreateNotFound(1));

        var queryHandler = provider.GetRequiredService<FirewallGetVlanIDsQueryHandler>();
        var request = new FirewallVlanIdFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().BeNull();
    }
}
