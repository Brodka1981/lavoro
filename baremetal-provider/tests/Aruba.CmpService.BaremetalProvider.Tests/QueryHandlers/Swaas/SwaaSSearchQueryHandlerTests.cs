using System.Collections.ObjectModel;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS.Wrappers;
using FluentAssertions;
using Moq;
using NSubstitute;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;


namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS;

public class SwaasSearchQueryHandlerTests : TestBase
{
    public SwaasSearchQueryHandlerTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var swaasService = Substitute.For<ISwaasesService>();
        services.AddSingleton(swaasService);
        services.AddSingleton<SwaasSearchCatalogQueryHandler>();
        services.AddSingleton<SwaasSearchQueryHandlerWrapper>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasService = provider.GetRequiredService<ISwaasesService>();

        swaasService.Search(It.IsAny<SwaasSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasList>()
            {
                Value = new SwaasList
                {
                    Values = new List<Swaas>()
                    {
                        new Swaas()
                        {
                           Id = "123",
                           CreatedBy = "aru-24468",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new SwaasProperties()
                        }
                    },
                }
            });

        var wrapper = provider.GetRequiredService<SwaasSearchQueryHandlerWrapper>();
        var request = new SwaasSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Values.Count.Should().Be(1);

        serverResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasService = provider.GetRequiredService<ISwaasesService>();

        swaasService.Search(It.IsAny<SwaasSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasList>()
            {
                Errors = new Collection<IServiceResultError>() { new FailureError() }
            });

        var wrapper = provider.GetRequiredService<SwaasSearchQueryHandlerWrapper>();
        var request = new SwaasSearchFilterRequest();

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = new SwaasCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest { Query = new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions()) };


        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest 
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().Name.Should().Be("Product 00 00");
        swaasResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Descending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().Name.Should().Be("Product 09 19");
        swaasResponse.Values.Last().Name.Should().Be("Product 00 00");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().Price.Should().Be(0);
        swaasResponse.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().Price.Should().Be(10 * 9 * 19);
        swaasResponse.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Networks_Count_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("networkcount", SortDirection.Ascending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().NetworksCount.Should().Be(0);
        swaasResponse.Values.Last().NetworksCount.Should().Be(4);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_OrderBy_Networks_Count_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("networkcount", SortDirection.Descending) }
            }
        };
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(200);
        swaasResponse.Values.Should().HaveCount(200);
        swaasResponse.Values.First().NetworksCount.Should().Be(4);
        swaasResponse.Values.Last().NetworksCount.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSwaas_Catalog_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var swaasesService = provider.GetRequiredService<ISwaasesService>();

        swaasesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SwaasCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SwaasSearchCatalogQueryHandler>();
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
        var swaasResponse = await wrapper.Handle(request);
        swaasResponse.Values.Should().NotBeNull();
        swaasResponse.TotalCount.Should().Be(1);
        swaasResponse.Values.Should().HaveCount(1);
    }

    private SwaasCatalog GetCatalog()
    {
        var elements = new List<SwaasCatalogItem>();

        for (var i = 0; i < 10; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                var element = new SwaasCatalogItem();
                element.Code = $"ProductCode_{i}_{x}";
                element.Name = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                element.DiscountedPrice = 20 * i * x;
                element.SetupFeePrice = 30 * i * x;
                element.NetworksCount = i % 5;
                elements.Add(element);
            }
        }
        var ret = new SwaasCatalog()
        {
            TotalCount = 200,
            Values = elements,

        };
        return ret;
    }
}
