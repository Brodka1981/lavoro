using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
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

public class ServerServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";

    public ServerServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<ServersService>>();
        services.AddSingleton(logger);

        var serversProvider = Substitute.For<IServersProvider>();
        services.AddSingleton(serversProvider);

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

        services.AddSingleton<IServersService, ServersService>();

        var renewFrequency = new RenewFrequencyOptions();
        for (byte i = 1; i < 12; i++)
        {
            renewFrequency.Add(i);
        }
        var renewFrequencyOptions = Options.Create<RenewFrequencyOptions>(renewFrequency);
        services.AddSingleton(renewFrequencyOptions);

        var baremetalOptions = Options.Create<BaremetalOptions>(new BaremetalOptions()
        {
            SddTimeLimit = 15
        });
        services.AddSingleton(baremetalOptions);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        var serverNameMapRepository = Substitute.For<IServerCatalogRepository>();
        serverNameMapRepository.GetServerCatalogAsync(It.IsAny<string>())
           .ReturnsForAnyArgs(new InternalServerCatalog()
           {
               Model = "AV - A105",
               ServerName = "AMD Ryzen 7000"
           });

        serverNameMapRepository.GetAllAsync()
           .ReturnsForAnyArgs<IEnumerable<InternalServerCatalog>>((IEnumerable<InternalServerCatalog?>)new List<InternalServerCatalog>{
               new InternalServerCatalog()
               {
                   Model = "AV - A105",
                   ServerName = "AMD Ryzen 7000"
               } });
        services.AddSingleton(serverNameMapRepository);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocationMapFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var locationMapRepository = provider.GetRequiredService<ILocationMapRepository>();

        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
            .ReturnsForAnyArgs((LocationMap)null);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var server = LegacyServerListItem;
        server.ServerFarmCode = null;
        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>(null));


        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            ProjectId = ProjectId,
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>>(
                new LegacyListResponse<LegacyServerListItem>
                {
                    Items = new List<LegacyServerListItem> { LegacyServerListItem },
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

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyServerListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await serverService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();


        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_Project_HttpError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Project>()
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_Project_Properties_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project()));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_Project_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(null));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a."
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit${"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit--server-name-edit--server-name-edit--server-name-edit--server-name-edit--server-name-edit--server-name-edit--"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }


    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Valid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH" } });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoApiError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "server-name-edit"
            }
        };

        var result = await serverService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Restart
    [Fact]
    [Unit]
    public async Task Restart_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);

    }

    [Fact]
    [Unit]
    public async Task Restart_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<ForbiddenError>();
    }

    [Fact]
    [Unit]
    public async Task Restart_InvalidProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            UserId = UserId,
            ResourceId = "123",
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Restart_InvalidServerId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            ProjectId = ProjectId,
            UserId = UserId,
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Restart_BadRequest()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Code" } });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Restart_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.Restart(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerRestartUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await serverService.Restart(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Update IpAddress
    [Fact]
    [Unit]
    public async Task UpdateIpAddress_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerUpdateIpUseCaseRequest()
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

        var result = await serverService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);

    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerUpdateIpUseCaseRequest()
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

        var result = await serverService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_ValidateFailed()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerUpdateIpUseCaseRequest()
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

        var result = await serverService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateIpAddress_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.UpdateIpAddress(It.IsAny<UpdateIpAddress>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerUpdateIpUseCaseRequest()
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

        var result = await serverService.UpdateIpAddress(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Search IpAddresses
    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var ipAddresses = Enumerable.Range(0, 3).Select(s => new LegacyIpAddress()
        {
            IpAddressId = s,
            CustomName = s % 2 == 0 ? "CustomName" : null,
            Hosts = new Collection<string>(),
            IpAddress = $"127.0.0.{s}",
            IpType = s
        });
        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Result = new LegacyListResponse<LegacyIpAddress>()
                {
                    TotalItems = ipAddresses.Count(),
                    Items = ipAddresses
                },
                Success = true
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(3);
        for (var i = 0; i < 3; i++)
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = "ERR_CLOUDDCS_REVERSEDNS_NOTFOUND"
                }
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeOfType<ServerIpAddressList>();
        result.Value.TotalCount.Should().Be(0);
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NullResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Result = null
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_LegacyResponse_StrangeErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = "StrangeError"
                }
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ErrCodeNull()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = true,
                Err = new ApiError()
                {
                    Code = null
                }
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchIpAddresses_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            ProjectId = ProjectId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
        };

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ProjectNotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(null));

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }



    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Project_NoProperties()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>(new Project()));

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }



    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Project_Default_Falsel()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
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

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_Project_HttpError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
        {
            UserId = UserId,
            ResourceId = 123,
            ProjectId = "1"
        };

        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
           .ReturnsForAnyArgs(new ApiCallOutput<Project>()
           {
               StatusCode = HttpStatusCode.InternalServerError
           });

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_ProjectProvider_ResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
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

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchIpAddresses_NoDefault_Project()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.SearchIpAddresses(It.IsAny<LegacySearchFilters>(), It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacyIpAddress>>()
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new ServerIpAddressesFilterRequest()
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

        var result = await serverService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest { Query = new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinition(new ResourceProvider.Common.ResourceQuery.ResourceQueryDefinitionOptions()),External=true };

        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
        result.Value.Values.First().Price.Should().Be(0);
        result.Value.Values.Last().Price.Should().Be(10 * 9 * 19);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Descending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("Name", SortDirection.Descending) }
            }
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    //    var serverProvider = provider.GetRequiredService<IServersProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    serverProvider.GetCatalog().Returns(catalogResponse);

    //    var serverService = provider.GetRequiredService<IServersService>();

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
    //    var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(1);
    //    result.Value.Values.Should().HaveCount(1);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_ErrorProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = false
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchCatalog_NullProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true
        };
        serverProvider.GetCatalog().Returns(catalogResponse);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await serverService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    #region Delete Plesk License
    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServerWithPleskLicense));

        serverProvider.DeletePleskLicense(It.IsAny<DeletePleskLicense>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new DeletePleskLicenseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.DeletePleskLicense(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_NoPleskLicenseCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        var legacyServerWithPleskLicense = LegacyServerWithPleskLicense;
        legacyServerWithPleskLicense.PleskLicensesInfo.First().LicenseCode = null;

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(legacyServerWithPleskLicense));

        serverProvider.DeletePleskLicense(It.IsAny<DeletePleskLicense>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new DeletePleskLicenseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.DeletePleskLicense(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServerWithPleskLicense));

        serverProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new DeletePleskLicenseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.DeletePleskLicense(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_WithoutPleskLicense()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServer));

        serverProvider.DeletePleskLicense(It.IsAny<DeletePleskLicense>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new DeletePleskLicenseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await serverService.DeletePleskLicense(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_BadRequestError_Invalid()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } };
        var result = await this.Delete_PleskLicense_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_BadRequestError_NoErrorCode()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() };
        var result = await this.Delete_PleskLicense_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_BadRequestError_NoApiError()
    {
        var apiOutput = new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
        var result = await this.Delete_PleskLicense_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Delete_PleskLicense_LegacyResponseFalse()
    {
        var apiOutput = new ApiCallOutput<bool>(false);
        var result = await this.Delete_PleskLicense_LegacyErrors(apiOutput);
        result.Errors.Should().HaveCount(1);
    }

    private async Task<ServiceResult> Delete_PleskLicense_LegacyErrors(ApiCallOutput<bool> response)
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serverProvider = provider.GetRequiredService<IServersProvider>();

        serverProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyServerDetail>(LegacyServerWithPleskLicense));

        serverProvider.DeletePleskLicense(It.IsAny<DeletePleskLicense>())
           .ReturnsForAnyArgs(response);

        var serverService = provider.GetRequiredService<IServersService>();

        var request = new DeletePleskLicenseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var ret = await serverService.DeletePleskLicense(request, CancellationToken.None).ConfigureAwait(false);
        return ret;
    }
    #endregion

    #region "Utils"

    private LegacyServerListItem LegacyServerListItem = new LegacyServerListItem()
    {
        Id = 123,
        Status = ServerStatuses.Active,
        Name = "server-name",
        ServerFarmCode = "Arezzo"
    };

    private LegacyServerDetail LegacyServer = new LegacyServerDetail()
    {
        Id = 123,
        Status = ServerStatuses.Active,
        Name = "server-name",
        ServerFarmCode = "Arezzo",
        ExpirationDate = DateTime.UtcNow,
    };

    private LegacyServerDetail LegacyServerWithPleskLicense = new LegacyServerDetail()
    {
        Id = 123,
        Status = ServerStatuses.Active,
        Name = "server-name",
        ServerFarmCode = "Arezzo",
        ExpirationDate = DateTime.UtcNow,
        PleskLicensesInfo = new List<LegacyServerPleskLicense>()
        {
            new LegacyServerPleskLicense()
            {
                ActivationCode="ActivationCode",
                ActivationDate=DateTimeOffset.UtcNow,
                Description ="Description",
                isAddon = false,
                LicenseCode = "LicenseCode"
            }
        }
    };

    #endregion
}
