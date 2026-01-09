
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using FluentAssertions;
using Moq;
using NSubstitute;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class SmartStorageQueryHandlersTests :
    TestBase
{
    private IServiceProvider _serviceProvider;
    public SmartStorageQueryHandlersTests(ITestOutputHelper output) : base(output)
    {
    }
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var smartStorageService = Substitute.For<ISmartStoragesService>();
        services.AddSingleton(smartStorageService);

        var projectProvider = Substitute.For<IProjectProvider>();
        services.AddSingleton(projectProvider);

        services.AddSingleton<GetAvailableSmartFoldersQueryHandler>();
        services.AddSingleton<SmartStorageGetByIdQueryHandler>();
        services.AddSingleton<SmartStorageProtocolsQueryHandler>();
        services.AddSingleton<SmartStorageSearchCatalogQueryHandler>();
        services.AddSingleton<SmartStorageSearchFoldersQueryHandler>();
        services.AddSingleton<SmartStorageSearchQueryHandler>();
        services.AddSingleton<SmartStorageSnapshotsQueryHandler>();
        services.AddSingleton<SmartStorageStatisticsQueryHandler>();
    }

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_Ok()
    {
        var service = this.SP.GetService<ISmartStoragesService>();
        service.GetAvailableSmartFolders(Arg.Any<GetAvailableSmartFoldersRequest>()).ReturnsForAnyArgs(new ServiceResult<GetAvailableSmartFoldersResponse>()
        {
            Value = new GetAvailableSmartFoldersResponse()
            {
                AvailableSmartFolders = 34
            }
        });

        var response = await service.GetAvailableSmartFolders(new GetAvailableSmartFoldersRequest()).ConfigureAwait(false);
        response.Should().NotBeNull();
        response.Value.Should().NotBeNull();
        response.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_Error()
    {
        var service = this.SP.GetService<ISmartStoragesService>();
        service.GetAvailableSmartFolders(Arg.Any<GetAvailableSmartFoldersRequest>()).ReturnsForAnyArgs(new ServiceResult<GetAvailableSmartFoldersResponse>()
        {
            Errors = new List<IServiceResultError>() { new FailureError() }
        });

        var response = await service.GetAvailableSmartFolders(new GetAvailableSmartFoldersRequest()).ConfigureAwait(false);
        response.Should().NotBeNull();
        response.Value.Should().BeNull();
        response.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = new SmartStorageCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest { Query = new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions()) };


        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest 
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };


        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
        smartStorageResponse.TotalCount.Should().Be(200);
        smartStorageResponse.Values.Should().HaveCount(200);
        smartStorageResponse.Values.First().Name.Should().Be("Product 00 00");
        smartStorageResponse.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Descending) }
            }
        };


        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
        smartStorageResponse.TotalCount.Should().Be(200);
        smartStorageResponse.Values.Should().HaveCount(200);
        smartStorageResponse.Values.First().Name.Should().Be("Product 09 19");
        smartStorageResponse.Values.Last().Name.Should().Be("Product 00 00");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };


        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
        smartStorageResponse.TotalCount.Should().Be(200);
        smartStorageResponse.Values.Should().HaveCount(200);
        smartStorageResponse.Values.First().Price.Should().Be(0);
        smartStorageResponse.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
        var request = new CatalogFilterRequest
        {
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };


        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
        smartStorageResponse.TotalCount.Should().Be(200);
        smartStorageResponse.Values.Should().HaveCount(200);
        smartStorageResponse.Values.First().Price.Should().Be(10 * 9 * 19);
        smartStorageResponse.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchSmartStorage_Catalog_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();

        smartStoragesService.SearchCatalog(It.IsAny<CatalogFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<SmartStorageCatalog>()
            {
                Value = GetCatalog()
            });

        var wrapper = provider.GetRequiredService<SmartStorageSearchCatalogQueryHandler>();
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
        var smartStorageResponse = await wrapper.Handle(request);
        smartStorageResponse.Values.Should().NotBeNull();
        smartStorageResponse.TotalCount.Should().Be(1);
        smartStorageResponse.Values.Should().HaveCount(1);
    }

    private IServiceProvider SP
    {
        get
        {
            if (this._serviceProvider == null)
            {
                this._serviceProvider = this.CreateServiceCollection().BuildServiceProvider();
            }
            return this._serviceProvider;
        }
    }

    private SmartStorageCatalog GetCatalog()
    {
        var elements = new List<SmartStorageCatalogItem>();

        for (var i = 0; i < 10; i++)
        {
            for (int x = 0; x < 20; x++)
            {
                var element = new SmartStorageCatalogItem();
                element.Code = $"ProductCode_{i}_{x}";
                element.Name = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                elements.Add(element);
            }
        }
        var ret = new SmartStorageCatalog()
        {
            TotalCount = 200,
            Values = elements,

        };
        return ret;
    }
}
