using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class SwitchQueryHandlersTests : TestBase
{
    public SwitchQueryHandlersTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<SwitchGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var logger2 = Substitute.For<ILogger<SwitchSearchCatalogQueryHandler>>();
        services.AddSingleton(logger2);

        var switchesService = Substitute.For<ISwitchesService>();

        services.AddSingleton(switchesService);

        services.AddSingleton<SwitchGetByIdQueryHandler>();
        services.AddSingleton<SwitchSearchQueryHandler>();
        services.AddSingleton<SwitchSearchCatalogQueryHandler>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetSwitchById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchesService = provider.GetRequiredService<ISwitchesService>();
        switchesService.GetById(new SwitchByIdRequest() { ResourceId = "123" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<Switch>()
            {
                Value = new Switch
                {
                    Id = "123",
                    CreatedBy = "aru-24468",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new SwitchProperties()
                }
            });

        var wrapper = provider.GetRequiredService<SwitchGetByIdQueryHandler>();
        var request = new SwitchByIdRequest()
        {
            ResourceId = "123"
        };

        var response = await wrapper.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetSwitchById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchesService = provider.GetRequiredService<ISwitchesService>();
        switchesService.GetById(It.IsAny<SwitchByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<Switch>.CreateNotFound("1"));


        var wrapper = provider.GetRequiredService<SwitchGetByIdQueryHandler>();
        var request = new SwitchByIdRequest()
        {
            ResourceId = "121"
        };

        var response = await wrapper.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.Search(It.IsAny<SwitchSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchList>()
            {
                Value = new SwitchList
                {
                    Values = new List<Switch>()
                    {
                        new Switch()
                        {
                           Id = "123",
                           CreatedBy = "aru-24468",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new SwitchProperties()
                        }
                    }
                }
            });

        var wrapper = provider.GetRequiredService<SwitchSearchQueryHandler>();
        var request = new SwitchSearchFilterRequest();

        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Count.Should().Be(1);
        switchResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.Search(It.IsAny<SwitchSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchList>.CreateNotFound("a"));

        var wrapper = provider.GetRequiredService<SwitchSearchQueryHandler>();
        var request = new SwitchSearchFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.Search(It.IsAny<SwitchSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchList>.CreateForbiddenError());

        var wrapper = provider.GetRequiredService<SwitchSearchQueryHandler>();
        var request = new SwitchSearchFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.Search(It.IsAny<SwitchSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchList>.CreateInternalServerError());

        var wrapper = provider.GetRequiredService<SwitchSearchQueryHandler>();
        var request = new SwitchSearchFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = new SwitchCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest { Query = new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions()) };


        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchCatalog>.CreateNotFound("a"));

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchCatalog>.CreateForbiddenError());

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<SwitchCatalog>.CreateInternalServerError());

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest();

        var switchResponse = await wrapper.Handle(request);

        switchResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest 
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
        switchResponse.TotalCount.Should().Be(200);
        switchResponse.Values.Should().HaveCount(200);
        switchResponse.Values.First().Name.Should().Be("Product 00 00");
        switchResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Descending) }
            }
        };
        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
        switchResponse.TotalCount.Should().Be(200);
        switchResponse.Values.Should().HaveCount(200);
        switchResponse.Values.First().Name.Should().Be("Product 09 19");
        switchResponse.Values.Last().Name.Should().Be("Product 00 00");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
        switchResponse.TotalCount.Should().Be(200);
        switchResponse.Values.Should().HaveCount(200);
        switchResponse.Values.First().Price.Should().Be(0);
        switchResponse.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
        switchResponse.TotalCount.Should().Be(200);
        switchResponse.Values.Should().HaveCount(200);
        switchResponse.Values.First().Price.Should().Be(10 * 9 * 19);
        switchResponse.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwitch_Catalog_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var switchesService = provider.GetRequiredService<ISwitchesService>();

        switchesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwitchCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwitchSearchCatalogQueryHandler>();
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
        var switchResponse = await wrapper.Handle(request);
        switchResponse.Values.Should().NotBeNull();
        switchResponse.TotalCount.Should().Be(1);
        switchResponse.Values.Should().HaveCount(1);
    }

    private SwitchCatalog GetCatalog()
    {
        var elements = new List<SwitchCatalogItem>();

        for (var i = 0; i < 10; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                var element = new SwitchCatalogItem();
                element.Code = $"ProductCode_{i}_{x}";
                element.Name = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                element.DiscountedPrice = 20 * i * x;
                element.SetupFeePrice = 30 * i * x;
                elements.Add(element);
            }
        }
        var ret = new SwitchCatalog()
        {
            TotalCount = 200,
            Values = elements,

        };
        return ret;
    }
}
