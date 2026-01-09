using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using ServerCatalog = Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers.ServerCatalog;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class ServerQueryHandlerTests : TestBase
{
    public ServerQueryHandlerTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<ServerGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var logger2 = Substitute.For<ILogger<ServerSearchCatalogQueryHandler>>();
        services.AddSingleton(logger2);

        var serversService = Substitute.For<IServersService>();

        services.AddSingleton(serversService);

        services.AddSingleton<ServerGetByIdQueryHandler>();
        services.AddSingleton<ServerSearchQueryHandler>();
        services.AddSingleton<ServerSearchCatalogQueryHandler>();
        services.AddSingleton<ServerSearchIpAddressesQueryHandler>();

    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetServerById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serversService = provider.GetRequiredService<IServersService>();
        serversService.GetById(new ServerByIdRequest() { ResourceId = "123" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<Server>()
            {
                Value = new Server
                {
                    Id = "123",
                    CreatedBy = "aru-24468",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new ServerProperties()
                }
            });

        var wrapper = provider.GetRequiredService<ServerGetByIdQueryHandler>();
        var request = new ServerByIdRequest()
        {
            ResourceId = "123"
        };

        var response = await wrapper.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetServerById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serversService = provider.GetRequiredService<IServersService>();
        serversService.GetById(It.IsAny<ServerByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<Server>.CreateNotFound("1"));


        var wrapper = provider.GetRequiredService<ServerGetByIdQueryHandler>();
        var request = new ServerByIdRequest()
        {
            ResourceId = "121"
        };

        var response = await wrapper.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchIpAddress_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serversService = provider.GetRequiredService<IServersService>();
        serversService.SearchIpAddresses(It.IsAny<ServerIpAddressesFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<ServerIpAddressList>()
            {
                Value = new ServerIpAddressList()
            });

        var queryHandler = provider.GetRequiredService<ServerSearchIpAddressesQueryHandler>();
        var request = new ServerIpAddressesFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();
        response.Values.Count.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchIpAddress_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serversService = provider.GetRequiredService<IServersService>();
        serversService.SearchIpAddresses(It.IsAny<ServerIpAddressesFilterRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(ServiceResult<ServerIpAddressList>.CreateNotFound(1));

        var queryHandler = provider.GetRequiredService<ServerSearchIpAddressesQueryHandler>();
        var request = new ServerIpAddressesFilterRequest();

        var response = await queryHandler.Handle(request);

        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.Search(It.IsAny<ServerSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerList>()
            {
                Value = new ServerList
                {
                    Values = new List<Server>()
                    {
                        new Server()
                        {
                           Id = "123",
                           CreatedBy = "aru-24468",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new ServerProperties()
                        }
                    }
                }
            });

        var wrapper = provider.GetRequiredService<ServerSearchQueryHandler>();
        var request = new ServerSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Count.Should().Be(1);
        serverResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.Search(It.IsAny<ServerSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerList>.CreateNotFound("a"));

        var wrapper = provider.GetRequiredService<ServerSearchQueryHandler>();
        var request = new ServerSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.Search(It.IsAny<ServerSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerList>.CreateForbiddenError());

        var wrapper = provider.GetRequiredService<ServerSearchQueryHandler>();
        var request = new ServerSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.Search(It.IsAny<ServerSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerList>.CreateInternalServerError());

        var wrapper = provider.GetRequiredService<ServerSearchQueryHandler>();
        var request = new ServerSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_OrderBy_Name_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value =GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest 
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(200);
        serverResponse.Values.Should().HaveCount(200);
        serverResponse.Values.First().Name.Should().Be("Product 00 00");
        serverResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(200);
        serverResponse.Values.Should().HaveCount(200);
        serverResponse.Values.First().Name.Should().Be("Product 00 00");
        serverResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Descending) }
            }
        };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(200);
        serverResponse.Values.Should().HaveCount(200);
        serverResponse.Values.First().Name.Should().Be("Product 09 19");
        serverResponse.Values.Last().Name.Should().Be("Product 00 00");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_OrderBy_Price__Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(200);
        serverResponse.Values.Should().HaveCount(200);
        serverResponse.Values.First().Price.Should().Be(0);
        serverResponse.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_OrderBy_Price__Descendingg()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(200);
        serverResponse.Values.Should().HaveCount(200);
        serverResponse.Values.First().Price.Should().Be(10 * 9 * 19);
        serverResponse.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
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

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
        serverResponse.TotalCount.Should().Be(1);
        serverResponse.Values.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<ServerCatalog>()
            {
                Value = new ServerCatalog()
            });

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest { Query = new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions()) };

        var serverResponse = await wrapper.Handle(request);
        serverResponse.Values.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerCatalog>.CreateNotFound("a"));

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerCatalog>.CreateForbiddenError());

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchServer_Catalog_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var serversService = provider.GetRequiredService<IServersService>();

        serversService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<ServerCatalog>.CreateInternalServerError());

        var wrapper = provider.GetRequiredService<ServerSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    private ServerCatalog GetCatalog()
    {
        var elements = new List<ServerCatalogItem>();

        for (var i = 0; i < 10; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                var element = new ServerCatalogItem();
                element.Code = $"ProductCode_{i}_{x}";
                element.Name = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                element.DiscountedPrice = 20 * i * x;
                element.SetupFeePrice = 30 * i * x;
                elements.Add(element);
            }
        }
        var ret = new ServerCatalog()
        {
            TotalCount = 200,
            Values = elements,

        };
        return ret;
    }
}
