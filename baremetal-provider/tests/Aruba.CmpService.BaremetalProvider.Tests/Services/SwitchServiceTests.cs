using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.MessageBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Project = Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects.Project;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services;

public class SwitchServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";

    public SwitchServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<SwitchesService>>();
        services.AddSingleton(logger);

        var swaasesProvider = Substitute.For<ISwitchesProvider>();
        services.AddSingleton(swaasesProvider);

        var locationMapRepository = Substitute.For<ILocationMapRepository>();
        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
           .ReturnsForAnyArgs(new LocationMap()
           {
               Id = "AR",
               LegacyValue = "Arezzo",
               Value = "ITAR-Arezzo"
           });
        services.AddSingleton(locationMapRepository);

        var projectProvider = Substitute.For<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project
            {
                Metadata = new ProjectMetadata()
                {
                    Id = Guid.NewGuid().ToString()
                },
                Properties = new ProjectProperties()
                {
                    Default = true
                }
            }));

        services.AddSingleton(projectProvider);

        var catalogueProvider = Substitute.For<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue("ITAR-Arezzo").Returns(new ApiCallOutput<Abstractions.Providers.Models.Location>(new Abstractions.Providers.Models.Location()
        {
            Id = Guid.NewGuid().ToString(),
            City = "Arezzo",
            Value = "ITAR-Arezzo",
            Abbreviation = "ITAR",
            Country = "Italy",
            CrowdInName = "Arezzo"
        }));

        var paymentsProvider = Substitute.For<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().Returns(new ApiCallOutput<bool>());
        services.AddSingleton(catalogueProvider);
        services.AddSingleton(paymentsProvider);

        var renewFrequency = new RenewFrequencyOptions { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var renewFrequencyOptions = Options.Create<RenewFrequencyOptions>(renewFrequency);
        services.AddSingleton(renewFrequencyOptions);

        var switchCatalogRepository = Substitute.For<ISwitchCatalogRepository>();
        services.AddSingleton(switchCatalogRepository);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        services.AddSingleton<ISwitchesService, SwitchesService>();
        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<UserProfile>(new UserProfile
            {
                IsResellerCustomer = true
            }));
        services.AddSingleton(profileProvider);
    }

    #region Search
    [Fact]
    [Unit]
    public async Task Search_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocationMapFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var locationMapRepository = provider.GetRequiredService<ILocationMapRepository>();

        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
            .ReturnsForAnyArgs((LocationMap)null);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
        result.Value.Values.First().Location.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task Search_ErroreLocationResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
        result.Value.Values.First().Location.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocation_Specified()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var @switch = LegacySwitchListItem;
        @switch.ServerFarmCode = null;
        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
        result.Value.Values.First().Location.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task Search_Location_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>(null));


        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
        result.Value.Values.First().Location.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task Search_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            ProjectId = ProjectId,
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_InvalidProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(null));

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_InvalidProjectProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>()
           {
               Success = false
           });

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectNonDefault()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project()
           {
               Properties = new ProjectProperties()
               {
                   Default = false
               }
           }));

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>(
                new LegacyListResponse<LegacySwitchListItem>
                {
                    Items = new List<LegacySwitchListItem> { LegacySwitchListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project
            {
                Metadata = new ProjectMetadata()
                {
                    Id = Guid.NewGuid().ToString()
                },
                Properties = new ProjectProperties()
                {
                    Default = false
                }
            }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwitchListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await switchService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();


        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project
            {
                Metadata = new ProjectMetadata()
                {
                    Id = Guid.NewGuid().ToString()
                },
                Properties = new ProjectProperties()
                {
                    Default = false
                }
            }));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a."
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await switchService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        //result.Value.Values.First().Price.Should().Be(0);
        //result.Value.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        //result.Value.Values.First().Price.Should().Be(10 * 9 * 19);
        //result.Value.Values.Last().Price.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        result.Value.Values.First().Name.Should().Be("Product 00 00");
        result.Value.Values.Last().Name.Should().Be("Product 09 19");
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Name_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("Name", SortDirection.Descending) }
            }
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        //result.Value.Values.First().Name.Should().Be("Product 09 19");
        //result.Value.Values.Last().Name.Should().Be("Product 00 00");
    }

    //[Fact]
    //[Unit]
    //public async Task SearchCatalog_Success_WithFullTextSearch()
    //{
    //    var provider = CreateServiceCollection().BuildServiceProvider();

    //    var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    switchProvider.GetCatalog().Returns(catalogResponse);

    //    var switchService = provider.GetRequiredService<ISwitchesService>();

    //    var request = new CatalogFilterRequest()
    //    {
    //        External = true,
    //        Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
    //        {
    //            Filters = new FilterDefinitionsList()
    //            {
    //                FilterDefinition.Create("fulltextsearch","eq","Product 00 11")
    //            }
    //        }
    //    };
    //    var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(1);
    //    result.Value.Values.Should().HaveCount(1);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_ErrorProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = false
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchCatalog_NullProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true
        };
        switchProvider.GetCatalog().Returns(catalogResponse);

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await switchService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    private IEnumerable<LegacyCatalog> GetCatalog()
    {
        var ret = new List<LegacyCatalog>();
        for (var i = 0; i < 10; i++)
        {
            var elements = new List<LegacyCatalogItem>();
            for (int x = 0; x < 20; x++)
            {
                var element = new LegacyCatalogItem();
                element.ProductCode = $"ProductCode_{i}_{x}";
                element.DisplayName = $"Product {i.ToString("00", CultureInfo.InvariantCulture)} {x.ToString("00", CultureInfo.InvariantCulture)}";
                element.IsSoldOut = false;
                element.Price = 10 * i * x;
                element.DiscountedPrice = 20 * i * x;
                element.SetupFeePrice = 30 * i * x;
                if (x % 10 != 0)
                {
                    element.BaseConfigProducts = Enumerable.Range(0, 3).Select(s => new LegacyCatalogItemConfigProduct()
                    {
                        ProductCode = $"ConfigCode_{i}_{x}_{s}",
                        ProductName = $"ConfigName {i} {x} {s}",
                        Quantity = s
                    }).ToList();
                }
                else
                {
                    element.BaseConfigProducts = null;
                }
                elements.Add(element);
            }
            var categoryItem = new LegacyCatalog()
            {
                Code = $"Category {i}",
                Elements = elements,

            };
            ret.Add(categoryItem);
        }
        return ret;
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit{}"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit-fdksfjdksfjdkjfkldsjfkldsjkfldjskfjdklsfjdklsjfkldsjfkljskfds-"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }


    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Valid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH" } });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoApiError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest });

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var switchProvider = provider.GetRequiredService<ISwitchesProvider>();

        switchProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwitchDetail>(LegacySwitch));

        switchProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var switchService = provider.GetRequiredService<ISwitchesService>();

        var request = new SwitchRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "switch-name-edit"
            }
        };

        var result = await switchService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region "Utils"
    private LegacySwitchListItem LegacySwitchListItem = new LegacySwitchListItem()
    {
        Id = 123,
        Status = SwitchStatuses.Active,
        Name = "switch-name",
    };

    private LegacySwitchDetail LegacySwitch = new LegacySwitchDetail()
    {
        Id = 123,
        Status = SwitchStatuses.Active,
        Name = "switch-name",
        ServerFarmCode = "Arezzo",
    };

    private LegacyCatalog legacyCatalog = new LegacyCatalog()
    {
        Code = "123",
        Elements = new List<LegacyCatalogItem>
        {
            new LegacyCatalogItem
            {
                Category="switch",
                DiscountedPrice=0,
                IsSoldOut=false,
                Price=0,
                SetupFeePrice=0,
                BaseConfigProducts=new List<LegacyCatalogItemConfigProduct>
                {
                    new LegacyCatalogItemConfigProduct
                    {
                        ProductCode="1",
                        ProductName="prodotto1",
                        Quantity=1
                    }
                }

            }
        },
    };
    #endregion
}
