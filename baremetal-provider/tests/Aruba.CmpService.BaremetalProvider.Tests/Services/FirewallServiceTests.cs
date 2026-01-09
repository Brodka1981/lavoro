using System.Collections.ObjectModel;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
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

public class FirewallServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";

    public FirewallServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<FirewallsService>>();
        services.AddSingleton(logger);

        var firewallsProvider = Substitute.For<IFirewallsProvider>();
        services.AddSingleton(firewallsProvider);

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

        var firewallCatalogRepository = Substitute.For<IFirewallCatalogRepository>();
        services.AddSingleton(firewallCatalogRepository);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        services.AddSingleton<IFirewallsService, FirewallsService>();

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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocationMapFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var locationMapRepository = provider.GetRequiredService<ILocationMapRepository>();

        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
            .ReturnsForAnyArgs((LocationMap)null);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var firewall = LegacyFirewallListItem;
        firewall.ServerFarmCode = null;
        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>(null));


        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            ProjectId = ProjectId,
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>(
                new LegacyListResponse<LegacyFirewallListItem>
                {
                    Items = new List<LegacyFirewallListItem> { LegacyFirewallListItem },
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

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await firewallService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();


        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a."
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await firewallService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name edit"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit${"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit--firewall-name-edit--firewall-name-edit--firewall-name-edit--firewall-name-edit--firewall-name-edit--"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "firewall-name-edit"
            }
        };

        var result = await firewallService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Update IpAddress
    [Fact]
    [Unit]
    public async Task UpdateIpAddress_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallUpdateIpUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Id = 456,
            IpAddress = new Abstractions.Dtos.IpAddressDto()
            {
                Description = "ip address",
                HostNames = new Collection<string>()
            }
        };

        var result = await firewallService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallUpdateIpUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Id = 456,
            IpAddress = new Abstractions.Dtos.IpAddressDto()
            {
                Description = "ip address",
                HostNames = new Collection<string>()
            }
        };

        var result = await firewallService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_ValidateFailed()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallUpdateIpUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = null,
            ResourceId = "123",
            Id = 456,
            IpAddress = new Abstractions.Dtos.IpAddressDto()
            {
                Description = "ip address",
                HostNames = new Collection<string>()
            }
        };

        var result = await firewallService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyFirewallDetail>(LegacyFirewall));

        firewallProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallUpdateIpUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Id = 456,
            IpAddress = new Abstractions.Dtos.IpAddressDto()
            {
                Description = "ip address",
                HostNames = new Collection<string>()
            }
        };

        var result = await firewallService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }
    #endregion

    #region Search IpAddresses
    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var ipAddresses = Enumerable.Range(0, 2).Select(s => new LegacyIpAddress()
        {
            IpAddressId = s,
            CustomName = s % 2 == 0 ? "CustomName" : null,
            Hosts = new Collection<string>(),
            IpAddress = $"127.0.0.{s}",
            IpType = s
        });
        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Result = new LegacyListResponse<LegacyIpAddress>()
                {
                    TotalItems = ipAddresses.Count(),
                    Items = ipAddresses
                },
                Success = true
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(ipAddresses.Count());
        for (var i = 0; i < ipAddresses.Count(); i++)
        {
            result.Value.Values.ElementAt(i).Description.Should().Be(ipAddresses.ElementAt(i).CustomName);
            result.Value.Values.ElementAt(i).Id.Should().Be(ipAddresses.ElementAt(i).IpAddressId);
            result.Value.Values.ElementAt(i).HostNames.Should().HaveCount(ipAddresses.ElementAt(i).Hosts.Count());
            result.Value.Values.ElementAt(i).Ip.Should().Be(ipAddresses.ElementAt(i).IpAddress);
            result.Value.Values.ElementAt(i).Type.Should().Be((IpAddressTypes)ipAddresses.ElementAt(i).IpType);
        }
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = "ERR_CLOUDDCS_REVERSEDNS_NOTFOUND"
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeOfType<FirewallIpAddressList>();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ErrCodeNull()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = null
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NullResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Result = null
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Result_Items_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Result = new LegacyListResponse<LegacyIpAddress>
                {
                    Items = null
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeOfType<FirewallIpAddressList>();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Values.Should().HaveCount(0);
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_LegacyResponse_StrangeErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = "StrangeError"
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
        };

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ProjectNotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(null));

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ProjectProvider_ResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = ProjectId
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>()
           {
               Success = false
           });

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoDefault_Project()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = ProjectId
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project()
           {
               Properties = new ProjectProperties()
               {
                   Default = false
               }
           }));

        var result = await firewallService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetVlanIds
    [Fact]
    [Unit]
    public async Task GetVlanIds_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var vlanids = Enumerable.Range(0, 2).Select(v => new LegacyVlanId()
        {
            Vlanid = string.Format("10{0}", v.ToString())
        });

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Result = new LegacyListResponse<LegacyVlanId>()
                {
                    TotalItems = vlanids.Count(),
                    Items = vlanids
                },
                Success = true
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(vlanids.Count());
        for (var i = 0; i < vlanids.Count(); i++)
        {
            result.Value.Values.ElementAt(i).Vlanid.Should().Be(vlanids.ElementAt(i).Vlanid);
        }
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_ErrCodeNull()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = null
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_NullResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = true,
                Result = null
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_Result_Items_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = true,
                Result = new LegacyListResponse<LegacyVlanId>
                {
                    Items = null
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeOfType<FirewallVlanIdList>();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Values.Should().HaveCount(0);
    }


    [Fact]
    [Unit]
    public async Task GetVlanIds_LegacyResponse_StrangeErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = "StrangeError"
                }
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task GetVlanIds_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_NoUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_NoProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
        };

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_ProjectNotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(null));

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_ProjectProvider_ResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = ProjectId
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>()
           {
               Success = false
           });

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVlanIds_NoDefault_Project()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        firewallProvider.GetVlanIds(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyVlanId>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new FirewallVlanIdFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = ProjectId
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project()
           {
               Properties = new ProjectProperties()
               {
                   Default = false
               }
           }));

        var result = await firewallService.GetVlanIds(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        firewallProvider.GetCatalog().Returns(catalogResponse);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        firewallProvider.GetCatalog().Returns(catalogResponse);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        result.Value.Values.First().Price.Should().Be(0);
        result.Value.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    //[Fact]
    //[Unit]
    //public async Task SearchCatalog_Success_OrderBy_Price_Descending()
    //{
    //    var provider = CreateServiceCollection().BuildServiceProvider();

    //    var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    firewallProvider.GetCatalog().Returns(catalogResponse);

    //    var firewallService = provider.GetRequiredService<IFirewallsService>();

    //    var request = new CatalogFilterRequest()
    //    {
    //        External = false,
    //        Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
    //        {
    //            Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
    //        }
    //    };
    //    var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(200);
    //    result.Value.Values.Should().HaveCount(200);
    //    result.Value.Values.First().Price.Should().Be(10 * 9 * 19);
    //    result.Value.Values.Last().Price.Should().Be(0);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Name_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        firewallProvider.GetCatalog().Returns(catalogResponse);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        result.Value.Values.First().Name.Should().Be("Product 00 00");
        result.Value.Values.Last().Name.Should().Be("Product 09 19");
    }

    //[Fact]
    //[Unit]
    //public async Task SearchCatalog_Success_OrderBy_Name_Descending()
    //{
    //    var provider = CreateServiceCollection().BuildServiceProvider();

    //    var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    firewallProvider.GetCatalog().Returns(catalogResponse);

    //    var firewallService = provider.GetRequiredService<IFirewallsService>();

    //    var request = new CatalogFilterRequest()
    //    {
    //        External = false,
    //        Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
    //        {
    //            Sorts = new SortDefinitionsList() { SortDefinition.Create("Name", SortDirection.Descending) }
    //        }
    //    };
    //    var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(200);
    //    result.Value.Values.Should().HaveCount(200);
    //    result.Value.Values.First().Name.Should().Be("Product 09 19");
    //    result.Value.Values.Last().Name.Should().Be("Product 00 00");
    //}

    //[Fact]
    //[Unit]
    //public async Task SearchCatalog_Success_WithFullTextSearch()
    //{
    //    var provider = CreateServiceCollection().BuildServiceProvider();

    //    var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    firewallProvider.GetCatalog().Returns(catalogResponse);

    //    var firewallService = provider.GetRequiredService<IFirewallsService>();

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
    //    var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(1);
    //    result.Value.Values.Should().HaveCount(1);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_ErrorProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = false
        };
        firewallProvider.GetCatalog().Returns(catalogResponse);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchCatalog_NullProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var firewallProvider = provider.GetRequiredService<IFirewallsProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true
        };
        firewallProvider.GetCatalog().Returns(catalogResponse);

        var firewallService = provider.GetRequiredService<IFirewallsService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await firewallService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    #region "Utils"

    private LegacyFirewallListItem LegacyFirewallListItem = new LegacyFirewallListItem()
    {
        Id = 123,
        ActivationDate = DateTime.UtcNow,
        Status = FirewallStatuses.Active,
        Name = "firewall-name",
        ServerFarmCode = "Arezzo",
        ExpirationDate = DateTime.UtcNow,
    };

    private LegacyFirewallDetail LegacyFirewall = new LegacyFirewallDetail()
    {
        Id = 123,
        ActivationDate = DateTime.UtcNow,
        Status = FirewallStatuses.Active,
        Name = "firewall-name",
        ServerFarmCode = "Arezzo",
        ExpirationDate = DateTime.UtcNow,
    };

    #endregion
}
