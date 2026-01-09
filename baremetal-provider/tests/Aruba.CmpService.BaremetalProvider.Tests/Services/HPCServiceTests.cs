using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
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

public class HPCServiceTests : TestBase
{
    private const string UserId = "ARU-25198";
    private const string ProjectId = "679a59350c011157344272a8";

    public HPCServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<HPCsService>>();
        services.AddSingleton(logger);

        var hpcsProvider = Substitute.For<IHPCsProvider>();
        hpcsProvider.GetHPCCatalog().ReturnsForAnyArgs(GetHPCCatalog());
        services.AddSingleton(hpcsProvider);

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
        services.AddSingleton(catalogueProvider);

        var paymentsProvider = Substitute.For<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().Returns(new ApiCallOutput<bool>());
        services.AddSingleton(catalogueProvider);
        services.AddSingleton(paymentsProvider);

        var renewFrequency = new RenewFrequencyOptions { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var renewFrequencyOptions = Options.Create<RenewFrequencyOptions>(renewFrequency);
        services.AddSingleton(renewFrequencyOptions);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        services.AddSingleton<IFirewallsService, FirewallsService>();
        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
        .ReturnsForAnyArgs(new ApiCallOutput<UserProfile>(new UserProfile
        {
            IsResellerCustomer = true
        }));

        var hpcRepository = Substitute.For<IHPCCatalogRepository>();
        hpcRepository.GetAllAsync().ReturnsForAnyArgs(GetCatalogRepository());
        services.AddSingleton(hpcRepository);

        services.AddSingleton(profileProvider);

        var paymentsService = Substitute.For<IPaymentsService>();
        services.AddSingleton(paymentsService);

        services.AddSingleton<IHPCsService, HPCsService>();

    }


    #region Search
    [Fact]
    [Unit]
    public async Task Search_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocationMapFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var locationMapRepository = provider.GetRequiredService<ILocationMapRepository>();

        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
            .ReturnsForAnyArgs((LocationMap)null);

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        var hpc = LegacyHPCListItem;
        hpc.ServerFarmCode = null;
        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>(null));


        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            ProjectId = ProjectId,
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>(
                new LegacyListResponse<LegacyHPCListItem>
                {
                    Items = new List<LegacyHPCListItem> { LegacyHPCListItem },
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

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyHPCListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();


        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a."
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit${"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit--hpc-name-edit--hpc-name-edit--hpc-name-edit--hpc-name-edit--hpc-name-edit--"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "hpc-name-edit"
            }
        };

        var result = await hpcService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Set Automatic Renew
    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001",
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidPaymentMethod_NullValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidPaymentMethod_SDDValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        var LegacyHPC = this.LegacyHPC;
        LegacyHPC.ExpirationDate = DateTime.UtcNow.AddDays(-5);

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001004"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_ValidPaymentMethod_SDDValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        var LegacyHPC = this.LegacyHPC;
        LegacyHPC.ExpirationDate = DateTime.UtcNow.AddDays(16);

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001004"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_ValidPaymentMethod_OtherValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        var LegacyHPC = this.LegacyHPC;
        LegacyHPC.ExpirationDate = DateTime.UtcNow.AddDays(-15);
        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidMonthValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 14,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidMonthNullValue()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidFraudSuccess()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } });
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidIsFraudUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(true));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var result = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<ForbiddenError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_LegacyResponseError()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError };
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_LegacyResponse_FalseResult()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = true, Result = false };
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_BadRequestError_Invalid()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } };
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_BadRequestError_NoErrorCode()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() };
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_BadRequestError_NoApiError()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_LegacyResponseFalse()
    {
        var apiOutput = new ApiCallOutput<bool>(false);
        var result = await this.SetAutomaticRenew_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
    }

    private async Task<ServiceResult> SetAutomaticRenew_LegacyErrors(ApiCallOutput<bool> response)
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcProvider = provider.GetRequiredService<IHPCsProvider>();

        hpcProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyHPCDetail>(LegacyHPC));

        hpcProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(response);

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new HPCSetAutomaticRenewUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Activate = true,
            Months = 1,
            PaymentMethodId = "001001"
        };

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));
        var ret = await hpcService.SetAutomaticRenew(request, CancellationToken.None).ConfigureAwait(false);
        return ret;
    }
    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await hpcService.SearchCatalog(request, CancellationToken.None);
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Values.Should().HaveCount(2);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await hpcService.SearchCatalog(request, CancellationToken.None);

        result.Value!.TotalCount.Should().Be(2);
        result.Value.Values.Should().HaveCount(2);
        result.Value.Values.First().Price.Should().Be((decimal)1890.0);
        result.Value.Values.Last().Price.Should().Be((decimal)5402.0);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var result = await hpcService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Values.Should().HaveCount(2);
        result.Value.Values.First().Price.Should().Be((decimal)5402.0);
        result.Value.Values.Last().Price.Should().Be((decimal)1890.0);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Filters = new FilterDefinitionsList()
                {
                    FilterDefinition.Create("fulltextsearch","eq","ABC")
                }
            }
        };
        var result = await hpcService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Values.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_NoResult_WithFullTextSearch()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcService = provider.GetRequiredService<IHPCsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Filters = new FilterDefinitionsList()
                {
                    FilterDefinition.Create("fulltextsearch","eq","NORESULT")
                }
            }
        };
        var result = await hpcService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(0);
        result.Value.Values.Should().HaveCount(0);
    }

    #endregion

    #region "Utils"

    private LegacyHPCListItem LegacyHPCListItem = new LegacyHPCListItem()
    {
        Id = 123,
        ActivationDate = DateTime.UtcNow,
        Status = HPCStatuses.Active,
        Name = "HPC-name",
        ServerFarmCode = "Arezzo",
        ExpirationDate = DateTime.UtcNow,
    };

    private LegacyHPCDetail LegacyHPC = new LegacyHPCDetail()
    {
        Id = 123,
        ActivationDate = DateTime.UtcNow,
        Status = HPCStatuses.Active,
        Name = "HPC-name",
        ExpirationDate = DateTime.UtcNow,
    };

    private static Task<ApiCallOutput<List<HPCConfiguration>>> GetHPCCatalog()
    {
        return Task.FromResult(new ApiCallOutput<List<HPCConfiguration>>()
        {
            Success = true,
            Result = GetHPCConfigurations()
        });
    }

    private static List<HPCConfiguration> GetHPCConfigurations()
    {
        var ret = new List<HPCConfiguration>();

        ret.Add(new HPCConfiguration()
        {
            BundleConfigurationCode = "HPC_CONF_01",
            ModuleType = BundleServiceModuleType.Server,
            Price = (decimal)1890.0,
            ProductCode = "HPCPRA104_22D",
            Quantity = 3,
        });

        ret.Add(new HPCConfiguration()
        {
            BundleConfigurationCode = "HPC_CONF_01",
            ModuleType = BundleServiceModuleType.Firewall,
            Price = (decimal)1890.0,
            ProductCode = "FW3_22D",
            Quantity = 1,
        });

        ret.Add(new HPCConfiguration()
        {
            BundleConfigurationCode = "HPC_CONF_02",
            ModuleType = BundleServiceModuleType.Server,
            Price = (decimal)5402,
            ProductCode = "HPCPRA104_22D",
            Quantity = 3,
        });

        ret.Add(new HPCConfiguration()
        {
            BundleConfigurationCode = "HPC_CONF_02",
            ModuleType = BundleServiceModuleType.Firewall,
            Price = (decimal)5402,
            ProductCode = "FW3_22D",
            Quantity = 1,
        });

        return ret;
    }

    private static Task<IEnumerable<InternalHPCCatalog>> GetCatalogRepository()
    {
        return Task.FromResult(CreateCatalogEntities());
    }

    private static IEnumerable<InternalHPCCatalog> CreateCatalogEntities()
    {
        var ret = new List<InternalHPCCatalog>();

        ret.Add(new InternalHPCCatalog()
        {
            Category = "HPCSERVER",
            BundleConfigurationCode = "HPC_CONF_01",
            Data = CreateServerData("ABC"),
            Firewall = new InternalHPCCatalogFirewall()
            {
                Model = "Firewall",
                Data = CreateFirewallData(),
            },
            Model = "Model",
            ServerName = "ServerNameABC",
        });

        ret.Add(new InternalHPCCatalog()
        {
            Category = "HPCSERVER",
            BundleConfigurationCode = "HPC_CONF_02",
            Data = CreateServerData("XYZ"),
            Firewall = new InternalHPCCatalogFirewall()
            {
                Model = "Firewall",
                Data = CreateFirewallData(),
            },
            Model = "Model",
            ServerName = "ServerNameXYZ",
        });

        return ret;
    }

    private static IEnumerable<InternalHPCFirewallData> CreateFirewallData()
    {
        var ret = new List<InternalHPCFirewallData>();

        ret.Add(new InternalHPCFirewallData()
        {
            FirewallName = "Firewall",
            Language = "it",
        });

        return ret;
    }

    private static IEnumerable<InternalHPCCatalogData> CreateServerData(string suffix)
    {
        var ret = new List<InternalHPCCatalogData>();

        ret.Add(new InternalHPCCatalogData()
        {
            Cpu = "Cpu",
            HardwareName = "HardwareName" + suffix,
            Hdd = "Hdd",
            Language = "it",
            NodeNumber = "NodeNumber" + suffix,
            Ram = "Ram"
        });

        return ret;
    }

    #endregion
}
