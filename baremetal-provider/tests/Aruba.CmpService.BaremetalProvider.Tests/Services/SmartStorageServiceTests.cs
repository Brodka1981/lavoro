using System.Globalization;
using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
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

public class SmartStorageServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";

    public SmartStorageServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<SmartStoragesService>>();
        services.AddSingleton(logger);

        var smartStoragesProvider = Substitute.For<ISmartStoragesProvider>();
        services.AddSingleton(smartStoragesProvider);

        var smartStorageCatalogRepository = Substitute.For<ISmartStorageCatalogRepository>();
        services.AddSingleton(smartStorageCatalogRepository);

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
            .ReturnsForAnyArgs(new ApiCallOutput<Project>(
                new Project
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
        paymentsProvider.GetFraudRiskAssessmentAsync().Returns(new ApiCallOutput<bool>(false) { Success = true });
        services.AddSingleton(catalogueProvider);
        services.AddSingleton(paymentsProvider);

        services.AddSingleton<ISmartStoragesService, SmartStoragesService>();

        var renewFrequency = new RenewFrequencyOptions { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var renewFrequencyOptions = Options.Create<RenewFrequencyOptions>(renewFrequency);
        services.AddSingleton(renewFrequencyOptions);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        var baremetalOptions = Options.Create<BaremetalOptions>(new BaremetalOptions()
        {
            SddTimeLimit = 15
        });
        services.AddSingleton(baremetalOptions);

        var enableUpdatedEventOptions = Options.Create<EnableUpdatedEventOptions>(new EnableUpdatedEventOptions()
        {
            Enable = true
        });
        services.AddSingleton(enableUpdatedEventOptions);

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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_NoLocationMapFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var locationMapRepository = provider.GetRequiredService<ILocationMapRepository>();

        locationMapRepository.GetLocationMapAsync(It.IsAny<string>())
            .ReturnsForAnyArgs((LocationMap)null);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var smartStorage = new LegacySmartStorageListItem();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>()
        {
            Success = false
        });


        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var catalogueProvider = provider.GetRequiredService<ICatalogueProvider>();
        catalogueProvider.GetLocationByValue(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Location>(null));


        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            ProjectId = ProjectId,
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
                    TotalItems = 1,
                    Offset = 0
                }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = "1",
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>(
                new LegacyListResponse<LegacySmartStorageListItem>
                {
                    Items = new List<LegacySmartStorageListItem> { LegacySmartStorageListItem },
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

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();


        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a."
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit${"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit--smartStorage-name-edit--smartStorage-name-edit--smartStorage-name-edit--smartStorage-name-edit--smartStorage-name-edit--smartStorage-name-edit--"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }


    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Valid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH" } });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoApiError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "smartStorage-name-edit"
            }
        };

        var result = await smartStorageService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Activate
    [Fact]
    [Unit]
    public async Task Activate_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageActivateUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageActivateDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.Activate(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Activate_False()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageActivateUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageActivateDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.Activate(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task Activate_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageActivateUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageActivateDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.Activate(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }
    [Fact]
    [Unit]
    public async Task Activate_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { StatusCode = HttpStatusCode.InternalServerError, Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageActivateUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageActivateDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.Activate(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    #endregion

    #region ChangePassword
    [Fact]
    [Unit]
    public async Task ChangePassword_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageChangePasswordDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.ChangePassword(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task ChangePassword_False()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageChangePasswordDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.ChangePassword(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task ChangePassword_ValidationError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageChangePasswordDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd",
            }
        };

        var result = await smartStorageService.ChangePassword(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task ChangePassword_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageChangePasswordDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.ChangePassword(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task ChangePassword_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ActivateSmartStorage(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { StatusCode = HttpStatusCode.InternalServerError, Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageChangePasswordUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ActivateData = new Abstractions.Dtos.SmartStorages.SmartStorageChangePasswordDto()
            {
                Password = "123asd!!",
                ConfirmPassword = "123asd!!",
            }
        };

        var result = await smartStorageService.ChangePassword(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().BeEmpty();
    }

    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("Name", SortDirection.Descending) }
            }
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    //    var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    smartStorageProvider.GetCatalog().Returns(catalogResponse);

    //    var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

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
    //    var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(1);
    //    result.Value.Values.Should().HaveCount(1);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_ErrorProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = false
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchCatalog_NullProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true
        };
        smartStorageProvider.GetCatalog().Returns(catalogResponse);

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await smartStorageService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    #region GetProtocols

    [Fact]
    [Unit]
    public async Task GetProtocols_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetProtolList(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyProtocol>>(new List<LegacyProtocol> { LegacyProtocolMock }));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageProtocolsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetProtocols(request).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value.Count().Should().Be(1);
    }

    [Fact]
    [Unit]
    public async Task GetProtocols_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetProtolList(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyProtocol>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageProtocolsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetProtocols(request).ConfigureAwait(false);
        result.Value.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task GetProtocols_NotValid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageProtocolsRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetProtocols(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region EnableProtocols

    [Fact]
    [Unit]
    public async Task EnableProtocol_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), true)
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageEnableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.EnableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
        result.Envelopes.Count().Should().Be(1);
    }

    [Fact]
    [Unit]
    public async Task EnableProtocol_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), true)
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageEnableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.EnableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task EnableProtocol_False()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), true)
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageEnableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.EnableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task EnableProtocol_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageEnableProtocolUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.EnableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    #endregion

    #region DisableProtocols

    [Fact]
    [Unit]
    public async Task DisableProtocol_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), false)
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDisableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.DisableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
        result.Envelopes.Count().Should().Be(1);
    }

    [Fact]
    [Unit]
    public async Task DisableProtocol_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), false)
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDisableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.DisableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task DisableProtocol_False()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.ToggleProtocol(It.IsAny<string>(), It.IsAny<ServiceType>(), true)
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDisableProtocolUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.DisableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task DisableProtocol_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDisableProtocolUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            ServiceType = ServiceType.Samba,
        };

        var result = await smartStorageService.DisableProtocol(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Count().Should().Be(0);
    }

    #endregion

    #region Statistics

    [Fact]
    [Unit]
    public async Task GetStatistics_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetStatistics(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyStatistics>(LegacyStatisticsMock));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageStatisticsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetStatistics(request).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
        result.Value.Should().NotBeNull();
        result.Value.TotalSnapshots.Should().Be(LegacyStatisticsMock.TotalSnapshots);
    }

    [Fact]
    [Unit]
    public async Task GetStatistics_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetStatistics(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyStatistics> { Success = false, StatusCode = HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageStatisticsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetStatistics(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Value.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task GetStatistics_NotValid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageStatisticsRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.GetStatistics(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Value.Should().BeNull();
    }

    #endregion

    #region SearchSmartFolders

    [Fact]
    [Unit]
    public async Task SearchFolders_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.SearchFolders(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchFolders_InvalidResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "invalidId"
        };

        var result = await smartStorageService.SearchFolders(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchFolders_ValidationFailed()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageFoldersRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123"
        };


        var result = await smartStorageService.SearchFolders(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SearchFoldersLegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.SearchFolders(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetAvailableSmartFolders

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new GetAvailableSmartFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetAvailableSmartFolders(request).ConfigureAwait(false);
        result.Value.AvailableSmartFolders.Should().Be(19);
    }

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_InvalidUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new GetAvailableSmartFoldersRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetAvailableSmartFolders(request).ConfigureAwait(false);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_InvalidResource()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new GetAvailableSmartFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "aa"
        };

        var result = await smartStorageService.GetAvailableSmartFolders(request).ConfigureAwait(false);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetAvailableSmartFolders_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { StatusCode = HttpStatusCode.InternalServerError, Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new GetAvailableSmartFoldersRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"
        };

        var result = await smartStorageService.GetAvailableSmartFolders(request).ConfigureAwait(false);
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region CreateSmartFolder

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "myfolder"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_InvalidName_MinLength()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "aa"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_InvalidName_MaxLength()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = new String('a', 101)
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_InvalidName_SpecialChar()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "{myfolder}"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_InvalidName_Space()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "myfolder "
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_InvalidUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "myfolder"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_False()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "myfolder"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "myfolder"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_NameAlreadyExists()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = HttpStatusCode.InternalServerError });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Name = "f111"
        };

        var result = await smartStorageService.CreateSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region DeleteSmartFolder

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_Success_NoSnapshots()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = new LegacySnapshots() });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_Success_WithSnapshots()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = new LegacySmartFolders(), Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.First().Should().BeOfType(typeof(NotFoundError));
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_SnapshotError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = new LegacySmartFolders(), Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = false });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.First().Should().BeOfType(typeof(NotFoundError));
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_DeleteSnapshotError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = false, Success = false, Err = new ApiError() });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_DeleteSnapshotTaskError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = false, Success = false, Err = new ApiError() });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_GetSnapshotsError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = null });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = false, Success = false, Err = new ApiError() });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_LegacyError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = false, Success = false, Err = new ApiError() });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_NoUSerId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_LegacyFalseResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = LegacySmartFolders, Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = false, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = "f111",
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.First().Should().BeOfType(typeof(FailureError));
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_NoSmartFolderId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Result = new LegacySmartFolders(), Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.DeleteSmartFolder(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>() { Result = true, Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteFolderUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SmartFolderId = null,
        };

        var result = await smartStorageService.DeleteSmartFolder(request).ConfigureAwait(false);
        result.Errors.First().Should().BeOfType(typeof(NotFoundError));
    }

    #endregion

    #region SetAutomaticRenew

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>());


        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_NoPaymentMethod()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "",
            Months = 2
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_NoMonths()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "1234001",
            Months = null
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_InvalidMonths()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "1234001",
            Months = 20
        };

        var result = await smartStorageService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region CreateSnapshot

    [Fact]
    [Unit]
    public async Task CreateSnapshot_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_SearchFolderErrors()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>() { Success = false });

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_NoFolder()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111222",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_GetSnapshotsError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = false, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_NoAvailableSnapshots()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var snapshotsResponse = LegacySnapshotsMock;
        snapshotsResponse.AvailableSnapshots = 0;

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = snapshotsResponse });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            FolderName = "f111",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region ApplySnapshot

    [Fact]
    [Unit]
    public async Task ApplySnapshot_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.RestoreSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageApplySnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.ApplySnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task ApplySnapshot_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.RestoreSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageApplySnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.ApplySnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task ApplySnapshot_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        smartStorageProvider.RestoreSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageApplySnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.ApplySnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region DeleteSnapshot

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_ValidationError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_NoSnapshotId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = null,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_LeacyInternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_LegacyError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshot(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = true });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotId = "snapshot123",
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshot(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region CreateSnapshotTask

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_Success_Hourly()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_GetSnapshotsError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = false });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidQuantity()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 200,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_NoSmartFolder()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111aaa",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = true });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_SearchFolderError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders> { Success = false });

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Hourly()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = 00,
                Hour = 23
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Hourly_NoMinutes()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Hourly,
                Minute = null,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Daily_NoHour()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Daily,
                Hour = null,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Daily_NoMinute()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Daily,
                Hour = 12,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Daily_DaysOfMonth()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Daily,
                Hour = 12,
                Minute = 00,
                DaysOfMonth = 1
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Weekly_NoDaysOfMonth()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Weekly,
                Hour = 12,
                Minute = 00,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Weekly_Month()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Weekly,
                Hour = 12,
                Minute = 00,
                DayOfWeek = DayOfWeek.Monday,
                Month = 1,
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Monthly_NoDaysOfMonth()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Monthly,
                DaysOfMonth = null
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_InvalidFormat_Monthly_DaysOfWeek()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSmartFolders(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartFolders>(LegacySmartFolders));

        smartStorageProvider.CreateSnapshotTask(It.IsAny<string>(), It.IsAny<SmartStorageCreateSnapshotTaskDto>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageCreateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            SnapshotTask = new SmartStorageCreateSnapshotTaskDto()
            {
                FolderName = "f111",
                Quantity = 1,
                LifeTimeUnitType = SnapshotLifeTimeUnitTypes.Monthly,
                DaysOfMonth = 1,
                Hour = 12,
                Minute = 00,
                Month = 1
            },
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.CreateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }


    #endregion

    #region UpdateSnapshotTask

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.UpdateSnapshotTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Enable = true,
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.UpdateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_SnapshotsNotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.UpdateSnapshotTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Enable = true,
            SnapshotId = 1,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.UpdateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_SnapshotsError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.UpdateSnapshotTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = false});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Enable = true,
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.UpdateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.UpdateSnapshotTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = true});

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Enable = true,
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.UpdateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.UpdateSnapshotTask(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false});

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageUpdateSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            Enable = true,
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.UpdateSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(0);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region DeleteSnapshotTask

    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshotTask(request).ConfigureAwait(false);
        result.Envelopes.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_ValidationError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotTaskUseCaseRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",             
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshotTask(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_NoSnapshotId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var result = await smartStorageService.DeleteSnapshotTask((SmartStorageDeleteSnapshotTaskUseCaseRequest)null).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().HaveCount(0);
    }
    
    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success= true});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshotTask(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.DeleteSnapshotTask(It.IsAny<string>(), It.IsAny<int>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success= false});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageDeleteSnapshotTaskUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
            SnapshotId = 123456789,
            MessageBusRequestId = Guid.NewGuid().ToString()
        };

        var result = await smartStorageService.DeleteSnapshotTask(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Envelopes.Should().HaveCount(0);
    }

    #endregion

    #region GetSnapshots

    [Fact]
    [Unit]
    public async Task GetSnapshots_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSnapshotsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
        };

        var result = await smartStorageService.GetSnapshots(request).ConfigureAwait(false);
        result.Value.Snapshots.Should().HaveCount(1);
        result.Value.SnapshotTasks.Should().HaveCount(1);
        result.Value.AvailableSnapshots.Should().Be(20);
        result.Value.TotalSnapshots.Should().Be(2);
    }

    [Fact]
    [Unit]
    public async Task GetSnapshots_ValidationError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSnapshotsRequest()
        {
            UserId = null,
            ProjectId = ProjectId,
            ResourceId = "123",             
        };

        var result = await smartStorageService.GetSnapshots(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetSnapshots_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = true, Result = null});

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSnapshotsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
        };

        var result = await smartStorageService.GetSnapshots(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetSnapshots_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStorageProvider = provider.GetRequiredService<ISmartStoragesProvider>();

        smartStorageProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySmartStorageDetail>(LegacySmartStorage));

        smartStorageProvider.GetSnapshots(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySnapshots>() { Success = false, Result = LegacySnapshotsMock });

        var smartStorageService = provider.GetRequiredService<ISmartStoragesService>();

        var request = new SmartStorageSnapshotsRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",             
        };

        var result = await smartStorageService.GetSnapshots(request).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region Utils

    private LegacySmartFolders LegacySmartFolders
        = new LegacySmartFolders()
        {
            AvailableSmartFolders = 19,
            UsedSmartFolders = 1,
            TotalSmartFolders = 20,
            SmartFolders = new List<LegacySmartFoldersItem>()
            {
                new LegacySmartFoldersItem()
                {
                     Name = "f111",
                     UsedSpace = "0GB",
                     Readonly = true,
                     AvailableSpace = "1GB",
                     IsRootFolder = true,
                     Position = "1",
                     PositionDisplay = "1",
                     RawAvailableSpace = 1000000000,
                     RawUsedSpace = 0
                }
            }
        };

    private LegacyStatistics LegacyStatisticsMock
        => new LegacyStatistics()
        {
            SmartFolders = new List<LegacyDataSet>
            {
                new LegacyDataSet{
                    Name ="TestDarioPM",
                    Size= "491MB",
                    RawSize= 514547712
                },
                new LegacyDataSet{
                    Name = "SSU99268710",
                    Size= "124KB",
                    RawSize= 126976
                },
                new LegacyDataSet{
                    Name= "TestDarioDEV",
                    Size= "96KB",
                    RawSize= 98304
                },
                new LegacyDataSet{
                    Name =  "TestFrancescoDev",
                    Size = "96KB",
                    RawSize= 98304
                }
            },
            Snapshots = new List<LegacySnapshot>{
                new LegacySnapshot{
                    Name= "manual_2025_03_04_11_18_11",
                    Size= "64.00KB",
                    SmartFolderName= "TestDarioDEV",
                    RawSize= 65536,
                    ReferencedSize= "96.00KB",
                    RawReferencedSize= 98304
                },
                new LegacySnapshot{
                    Name= "manual_2025_03_07_05_05_18",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                },
                new LegacySnapshot{
                    Name= "manual_2025_04_07_01_28_53",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                },
                new LegacySnapshot{
                    Name= "manual_2025_04_08_10_01_32",
                    Size= "0.00KB",
                    SmartFolderName= "TestDarioPM",
                    RawSize= 0,
                    ReferencedSize= "490.71MB",
                    RawReferencedSize= 514547712
                }
            },
            TotalSmartFolders = 4,
            RawTotalSmartFoldersSize = 514871296,
            TotalSmartFoldersSize = "491.02MB",
            TotalSnapshots = 4,
            RawTotalSnapshotsSize = 65536,
            TotalSnapshotsSize = "64.00KB",
            RawTotalDiskSpace = 10737418240,
            TotalDiskSpace = "10.00GB",
            RawAvailableDiskSpace = 6986108928,
            AvailableDiskSpace = "6.51GB",
            RawReservedDiskSpace = 0,
            ReservedDiskSpace = "0.00KB",
            TotalUsedSpace = "3.49GB",
            RawTotalUsedSpace = 3751309312
        };

    private LegacyProtocol LegacyProtocolMock = new LegacyProtocol()
    {
        ServiceStatus = LegacyServiceStatus.Running,
        ServiceType = LegacyServiceType.WebDav
    };

    private LegacySmartStorageDetail LegacySmartStorage = new LegacySmartStorageDetail()
    {
        Id = 123,
        Status = SmartStoragesStatuses.Active,
        Name = "server-name",
        ExpirationDate = DateTime.UtcNow,
        ActivationDate = DateTime.UtcNow,
        HasReplica = true,
    };

    private LegacySmartStorageListItem LegacySmartStorageListItem = new LegacySmartStorageListItem()
    {
        Id = 123,
        Status = SmartStoragesStatuses.Active,
        Name = "server-name",
        ActivationDate = DateTime.UtcNow,
        ExpirationDate = DateTime.UtcNow.AddMonths(1)
    };

    private LegacySnapshots LegacySnapshotsMock = new LegacySnapshots
    {
        AvailableSnapshots = 20,
        TotalSnapshots = 2,
        Snapshots = new List<LegacyManualSnapshots>
         {
             new LegacyManualSnapshots()
             {
                 SnapshotId = "snapshot123",
                 SmartFolderName = "f111"
             }
         },
        SnapshotTasks = new List<LegacySnapshotsTasks>()
         {
             new LegacySnapshotsTasks()
             {
                 SnapshotTaskId = 123456789,
                 SmartFolderName = "f111"
             }
         }
    };

    private List<LegacyProtocol> LegacyProtocolsMock = new List<LegacyProtocol>
    { 
        new LegacyProtocol()
        {
            ServiceType = LegacyServiceType.WebDav,
            ServiceStatus = LegacyServiceStatus.Running,
        }
    };
        #endregion
    }
