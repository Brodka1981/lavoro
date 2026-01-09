using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
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
using NSubstitute.Core;
using Project = Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects.Project;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services;
public class SwaaSesServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";
    public SwaaSesServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<SwaasesService>>();
        services.AddSingleton(logger);

        var swaasesProvider = Substitute.For<ISwaasesProvider>();
        services.AddSingleton(swaasesProvider);

        var swaasCatalogRepository = Substitute.For<ISwaasCatalogRepository>();
        swaasCatalogRepository.GetAllAsync().ReturnsForAnyArgs<IEnumerable<InternalSwaasCatalog>>(new List<InternalSwaasCatalog>{
               new InternalSwaasCatalog()
               {
                   Code = "ProductCode_0_0",
                   Data = new List<InternalSwaasCatalogData>()
                   {
                       new InternalSwaasCatalogData()
                       {
                           Language = "it",
                           Model = "Product 00 00"
                       }
                   }
               },
            new InternalSwaasCatalog()
               {
                   Code = "ProductCode_9_19",
                   Data = new List<InternalSwaasCatalogData>()
                   {
                       new InternalSwaasCatalogData()
                       {
                           Language = "it",
                           Model = "Product 09 19"
                       }
                   }
               } });

        services.AddSingleton(swaasCatalogRepository);

        var locationMapRepository = Substitute.For<ILocationMapRepository>();
        services.AddSingleton(locationMapRepository);

        var projectProvider = Substitute.For<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Projects.Project>(new Abstractions.Providers.Models.Projects.Project
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
        services.AddSingleton(catalogueProvider);

        var internalLegacyProvider = Substitute.For<IInternalLegacyProvider>();
        services.AddSingleton(internalLegacyProvider);

        var paymentsProvider = Substitute.For<IPaymentsProvider>();
        paymentsProvider.GetFraudRiskAssessmentAsync().Returns(new ApiCallOutput<bool>(false) { Success = true });
        services.AddSingleton(paymentsProvider);

        services.AddSingleton<ISwaasesService, SwaasesService>();
        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<UserProfile>(new UserProfile
            {
                IsResellerCustomer = true
            }));
        services.AddSingleton(profileProvider);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        var renewFrequency = new RenewFrequencyOptions { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var renewFrequencyOptions = Options.Create<RenewFrequencyOptions>(renewFrequency);
        services.AddSingleton(renewFrequencyOptions);
    }

    #region Search
    [Fact]
    [Unit]
    public async Task Search_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwaasListItem>>(
                new LegacyListResponse<LegacySwaasListItem>
                {
                    Items = new List<LegacySwaasListItem> { legacySwaasListItem },
                    TotalItems = 1
                }));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
        };

        var result = await swaasService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task Search_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwaasListItem>>(
                new LegacyListResponse<LegacySwaasListItem>
                {
                    Items = new List<LegacySwaasListItem> { legacySwaasListItem },
                    TotalItems = 1
                }));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSearchFilterRequest()
        {
            ProjectId = ProjectId
        };
        var result = await swaasService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwaasListItem>>(
                new LegacyListResponse<LegacySwaasListItem>
                {
                    Items = new List<LegacySwaasListItem> { legacySwaasListItem },
                    TotalItems = 1
                }));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSearchFilterRequest()
        {
            UserId = UserId
        };

        var result = await swaasService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_ProjectError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwaasListItem>>(
                new LegacyListResponse<LegacySwaasListItem>
                {
                    Items = new List<LegacySwaasListItem> { legacySwaasListItem },
                    TotalItems = 1
                }));

        var projectProvider = provider.GetRequiredService<IProjectProvider>();

        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<Abstractions.Providers.Models.Projects.Project>(new Abstractions.Providers.Models.Projects.Project
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

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await swaasService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Search_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacyListResponse<LegacySwaasListItem>> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSearchFilterRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await swaasService.Search(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region GetById
    [Fact]
    [Unit]
    public async Task GetById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"

        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123"

        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingProjectId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ResourceId = "123"

        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
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

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"

        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_MissingResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId
        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_NotLongResourceId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "a"
        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetById_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasByIdRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123"

        };

        var result = await swaasService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region Rename
    [Fact]
    [Unit]
    public async Task Rename_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidUserId()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidChars()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit${"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_InvalidName()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit--swaas-name-edit--swaas-name-edit--swaas-name-edit--swaas-name-edit--swaas-name-edit--swaas-name-edit--"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooLong()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameEmpty()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = " "
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_Name_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = null
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_RenameData_Null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = null
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_NameTooShort()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "ser"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.InternalServerError });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }


    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Valid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH" } });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<BadRequestError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_Invalid()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() { Code = "Invalid" } });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoErrorCode()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest, Err = new ApiError() });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponse_BadRequestError_NoApiError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool> { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<NotFoundError>();
    }

    [Fact]
    [Unit]
    public async Task Rename_LegacyResponseFalse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.Rename(It.IsAny<ResourceRename>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasRenameUseCaseRequest()
        {
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            RenameData = new Abstractions.Dtos.RenameDto()
            {
                Name = "swaas-name-edit"
            }
        };

        var result = await swaasService.Rename(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region SetAutomaticRenew

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.DeleteAutomaticRenew(It.IsAny<ResourceDeleteAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Disable_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>());


        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            ActionOnFolder = Abstractions.Models.Enums.AutorenewFolderAction.RemoveFromFolder,
            Activate = false,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        swaasProvider.UpsertAutomaticRenew(It.IsAny<ResourceUpsertAutomaticRenew>())
            .ReturnsForAnyArgs(new ApiCallOutput<bool>(false) { Success = false });

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "123001",
            Months = 2
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_NoPaymentMethod()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "",
            Months = 2
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_NoMonths()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "1234001",
            Months = null
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task SetAutomaticRenewInternal_Enable_InvalidMonths()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        swaasProvider.GetById(It.IsAny<long>())
            .ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new SwaasSetAutomaticRenewUseCaseRequest()
        {
            Activate = true,
            UserId = UserId,
            ProjectId = ProjectId,
            ResourceId = "123",
            PaymentMethodId = "1234001",
            Months = 20
        };

        var result = await swaasService.SetAutomaticRenew(request, default).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region Search Catalog
    [Fact]
    [Unit]
    public async Task SearchCatalog_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Value.TotalCount.Should().Be(200);
        result.Value.Values.Should().HaveCount(200);
    }

    [Fact]
    [Unit]
    public async Task SearchCatalog_Success_OrderBy_Price_Ascending()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Ascending) }
            }
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("price", SortDirection.Descending) }
            }
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("nAmE", SortDirection.Ascending) }
            }
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true,
            Result = GetCatalog()
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = false,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
            {
                Sorts = new SortDefinitionsList() { SortDefinition.Create("Name", SortDirection.Descending) }
            }
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    //    var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

    //    var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
    //    {
    //        Success = true,
    //        Result = GetCatalog()
    //    };
    //    swaasProvider.GetCatalog().Returns(catalogResponse);

    //    var swaasService = provider.GetRequiredService<ISwaasesService>();

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
    //    var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
    //    result.Value.TotalCount.Should().Be(1);
    //    result.Value.Values.Should().HaveCount(1);
    //}

    [Fact]
    [Unit]
    public async Task SearchCatalog_ErrorProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = false
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }


    [Fact]
    [Unit]
    public async Task SearchCatalog_NullProviderResponse()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasProvider = provider.GetRequiredService<ISwaasesProvider>();

        var catalogResponse = new ApiCallOutput<IEnumerable<LegacyCatalog>>()
        {
            Success = true
        };
        swaasProvider.GetCatalog().Returns(catalogResponse);

        var swaasService = provider.GetRequiredService<ISwaasesService>();

        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            })
        };
        var result = await swaasService.SearchCatalog(request, CancellationToken.None).ConfigureAwait(false);
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

    #region GetVirtualSwitches

    [Fact]
    [Unit]
    public async Task GetVirtualSwitches_Success_ReturnsMappedList()
    {
        var request = new VirtualSwitchSearchFilterRequest { SwaasId = "swaas-001" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitches(request);

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Values.First().Id.Should().Be("vswitch-bergamo");
        result.Value.Values.First().Location.Should().Be("Bergamo");
        result.Value.Values.First().LocationCodes.Should().BeEquivalentTo(new[] { "IT1", "IT2" });
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitches_LegacyResponseFailed()
    {
        var request = new VirtualSwitchSearchFilterRequest { SwaasId = "swaas-002" };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(null) { Success = false });

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(new List<LegacyRegion>()));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitches(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitches_LegacyResultNull()
    {
        var request = new VirtualSwitchSearchFilterRequest { SwaasId = "swaas-003" };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(null) { Success = true });

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(new List<LegacyRegion>()));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitches(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitches_RegionResponseFailed_ReturnsInternalServerError()
    {
        var request = new VirtualSwitchSearchFilterRequest { SwaasId = "swaas-004" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(null) { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitches(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitches_RegionResultNull_ReturnsInternalServerError()
    {
        var request = new VirtualSwitchSearchFilterRequest { SwaasId = "swaas-005" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(null));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitches(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }


    #endregion

    #region GetVirtualSwitch

    [Fact]
    [Unit]
    public async Task GetVirtualSwitch_Success()
    {
        var request = new VirtualSwitchGetByIdRequest { SwaasId = "swaas-005", VirtualSwitchId = "123a", UserId = UserId, ProjectId = "default" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitch(request);

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be("123a");
        result.Value.Location.Should().Be("Bergamo");
        result.Value.LocationCodes.Should().BeEquivalentTo(new[] { "IT1", "IT2" });
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitch_LegacyResponseFailed()
    {
        var request = new VirtualSwitchGetByIdRequest { SwaasId = "swaas-005", VirtualSwitchId = "123a", UserId = UserId, ProjectId = "default" };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(null) { Success = false });

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(new List<LegacyRegion>()));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitch(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitch_LegacyResultNull()
    {
        var request = new VirtualSwitchGetByIdRequest { SwaasId = "swaas-005", VirtualSwitchId = "123a", UserId = UserId, ProjectId = "default" };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(null) { Success = true });

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(new List<LegacyRegion>()));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitch(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitch_RegionResponseFailed_ReturnsInternalServerError()
    {
        var request = new VirtualSwitchGetByIdRequest { SwaasId = "swaas-005", VirtualSwitchId = "123a", UserId = UserId, ProjectId = "default" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(null) { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitch(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitch_RegionResultNull_ReturnsInternalServerError()
    {
        var request = new VirtualSwitchGetByIdRequest { SwaasId = "swaas-005", VirtualSwitchId = "123a", UserId = UserId, ProjectId = "default" };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(Arg.Any<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        internalLegacyProvider.GetRegions()
            .ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(null));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitch(request);

        result.Should().NotBeNull();
        result.ContinueCheckErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region AddVirtualSwitch

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_Success()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Location.Should().Be("Bergamo");
        result.Value.Id.Should().Be("123a");
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_Error()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>() { Success = false });

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_NullResponse()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>() { Success = true, Result = null });

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_LegacyRegionError()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>> { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_EmptyLocation()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>> { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_InvalidRegion()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_InvalidName()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = ""
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_InvalidNameLenght_Max()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = new String('a', 51)
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitch_InvalidProject()
    {
        var request = new VirtualSwitchAddUseCaseRequest
        {
            VirtualSwitch = new Abstractions.Dtos.Swaases.VirtualSwitchAddDto()
            {
                Location = "IT1-IT2",
                Name = "my-vs"
            },
            ResourceId = "724",
            UserId = UserId,
            ProjectId = null
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.AddVirtualSwitch(It.IsAny<AddLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region EditVirtualSwitch

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_Success()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Location.Should().Be("Bergamo");
        result.Value.Id.Should().Be("123a");
        result.Value.Name.Should().Be("my-vs");
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_RegionsError()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>() { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_NullError()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch> { Success = true, Result = null });

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_Error()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch> { Success = false });

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_InvalidName()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs€",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_NotFound()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123axxx",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var legacyRegions = new List<LegacyRegion> { new LegacyRegion { RegionId = "IT1-IT2", RegionName = "Bergamo" } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.EditVirtualSwitch(It.IsAny<EditLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput<LegacyVirtualSwitch>(legacyVirtualSwitch));

        internalLegacyProvider.GetRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyRegion>>(legacyRegions));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_GetVirtualSwitches_Error()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = UserId,
            ProjectId = "default"
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>> { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task EditVirtualSwitch_NoUserId()
    {
        var request = new VirtualSwitchEditUseCaseRequest
        {
            Id = "123a",
            Name = "my-vs",
            ResourceId = "724",
            UserId = null,
            ProjectId = "default"
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.EditVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region DeleteVirtualSwitch

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitch_Success()
    {
        var request = new VirtualSwitchDeleteUseCaseRequest
        {
            Id = "123a",
            ResourceId = "123",
            UserId = UserId,
            ProjectId = "default",
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetById(It.IsAny<long>()).ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.DeleteVirtualSwitch(It.IsAny<DeleteLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput() { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitch_Error()
    {
        var request = new VirtualSwitchDeleteUseCaseRequest
        {
            Id = "123a",
            ResourceId = "123",
            UserId = UserId,
            ProjectId = "default",
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetById(It.IsAny<long>()).ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.DeleteVirtualSwitch(It.IsAny<DeleteLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput() { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitch_NotFound()
    {
        var request = new VirtualSwitchDeleteUseCaseRequest
        {
            Id = "123axxxx",
            ResourceId = "123",
            UserId = UserId,
            ProjectId = "default",
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetById(It.IsAny<long>()).ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        legacyProvider.DeleteVirtualSwitch(It.IsAny<DeleteLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput() { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitch_GetVirtualSwitches_Error()
    {
        var request = new VirtualSwitchDeleteUseCaseRequest
        {
            Id = "123a",
            ResourceId = "123",
            UserId = UserId,
            ProjectId = "default",
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetById(It.IsAny<long>()).ReturnsForAnyArgs(new ApiCallOutput<LegacySwaasDetail>(LegacySwaas));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>() { Success = false });

        legacyProvider.DeleteVirtualSwitch(It.IsAny<DeleteLegacyVirtualSwitch>()).ReturnsForAnyArgs(new ApiCallOutput() { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitch_NoUserId()
    {
        var request = new VirtualSwitchDeleteUseCaseRequest
        {
            Id = "123a",
            ResourceId = "123",
            UserId = null,
            ProjectId = "default",
            MessageBusRequestId = Guid.NewGuid().ToString(),
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitch(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region GetLinkableServices

    [Fact]
    [Unit]
    public async Task GetLinkableServices_Success()
    {
        var request = new VirtualSwitchGetLinkableServicesRequest
        {
            SwaasId = "123",
            VirtualSwitchId = "123a",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };
        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 123 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "1234",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetLinkableServices(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.Count.Should().Be(1);
        result.Value.First().Name.Should().Be("server1");
    }

    [Fact]
    [Unit]
    public async Task GetLinkableServices_NoServices()
    {
        var request = new VirtualSwitchGetLinkableServicesRequest
        {
            SwaasId = "123",
            VirtualSwitchId = "123a",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };
        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT3", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 123 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "123",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetLinkableServices(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.Count.Should().Be(0);
    }

    [Fact]
    [Unit]
    public async Task GetLinkableServices_GetServicesWithRegions_Error()
    {
        var request = new VirtualSwitchGetLinkableServicesRequest
        {
            SwaasId = "123",
            VirtualSwitchId = "123a",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>() { Success = false });
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetLinkableServices(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetLinkableServices_NotFound()
    {
        var request = new VirtualSwitchGetLinkableServicesRequest
        {
            SwaasId = "123",
            VirtualSwitchId = "123xxxx",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };
        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 123 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetLinkableServices(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetLinkableServices_GetVirtualSwitches_Error()
    {
        var request = new VirtualSwitchGetLinkableServicesRequest
        {
            SwaasId = "123",
            VirtualSwitchId = "123a",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitch = new LegacyVirtualSwitch
        {
            VirtualNetworkId = "123a",
            FriendlyName = "vswitch-bergamo",
            Status = VirtualSwitchStatuses.Active,
            Region = "IT1-IT2"
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch> { legacyVirtualSwitch };
        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 123 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>() { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetLinkableServices(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region GetVirtualSwitchLinks

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLinks_Success()
    {
        var request = new VirtualSwitchLinkSearchFilterRequest
        {
            SwaasId = "123",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "123",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLinks(request);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.Values.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLinks_Empty()
    {
        var request = new VirtualSwitchLinkSearchFilterRequest
        {
            SwaasId = "123",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink>();

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLinks(request);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLinks_Error()
    {
        var request = new VirtualSwitchLinkSearchFilterRequest
        {
            SwaasId = "123",
            UserId = UserId,
            ProjectId = "default",
        };


        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>() { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLinks(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region GetVirtualSwitchLink

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_Success()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = "123",
            VirtualSwitchLinkId = "987",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "987",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.Id.Should().Be("987");
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_NotFound()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = "123",
            VirtualSwitchLinkId = "98743",
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "987",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_LegacyError()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = "123",
            VirtualSwitchLinkId = "987",
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>() { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_NoUser()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = "123",
            VirtualSwitchLinkId = "987",
            UserId = null,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_NoSwaasId()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = null,
            VirtualSwitchLinkId = "987",
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task GetVirtualSwitchLink_NoVirtualSwitchLinkId()
    {
        var request = new VirtualSwitchLinkGetByIdRequest
        {
            SwaasId = "123",
            VirtualSwitchLinkId = "",
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.GetVirtualSwitchLink(request);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region AddVirtualSwitchLink

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_Success()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo-12",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 1234 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        legacyProvider.AddVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LegacyServiceType>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Value.VirtualSwitchId.Should().Be("vswitch-bergamo-12");
        result.Value.LinkedServiceId.Should().Be(1234);
    }
    
    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_LegacyError()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo-12",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 1234 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        legacyProvider.AddVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LegacyServiceType>()).ReturnsForAnyArgs(new ApiCallOutput { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_RegionMismatch()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo-12",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT3"
            }
        };

        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 1234 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        legacyProvider.AddVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LegacyServiceType>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_GetServicesWithRegions_Error()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo-12",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>() { Success = false});
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        legacyProvider.AddVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LegacyServiceType>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_VirtualSwitchNotFound()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "xxxxxx",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitches = new List<LegacyVirtualSwitch>
        {
            new LegacyVirtualSwitch
            {
                VirtualNetworkId = "vswitch-bergamo-12",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var LegacyServicesWithRegions = new List<LegacyServiceWithRegion> { new LegacyServiceWithRegion { Region = "IT1", Name = "server1", ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server, Id = 1234 } };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        internalLegacyProvider.GetServicesWithRegions().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>(LegacyServicesWithRegions));
        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>(legacyVirtualSwitches));
        legacyProvider.AddVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<LegacyServiceType>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_GetVirtualSwitches_Error()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();
        var internalLegacyProvider = provider.GetRequiredService<IInternalLegacyProvider>();

        legacyProvider.GetVirtualSwitches(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitch>>() { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_NoProjectId()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = null,
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_NoLinkedServiceId()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = null,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = "server"
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task AddVirtualSwitchLink_NoLinkedServiceTypology()
    {
        var request = new VirtualSwitchLinkAddUseCaseRequest
        {
            ResourceId = "123",
            Link = new Abstractions.Dtos.Swaases.VirtualSwitchLinkAddDto()
            {
                LinkedServiceId = 1234,
                VirtualSwitchId = "vswitch-bergamo-12",
                LinkedServiceTypology = null
            },
            UserId = UserId,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.AddVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }
    #endregion

    #region DeleteVirtualSwitchLink

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitchLink_Success()
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest
        {
            Id = "123aaa",  // vsl id
            ResourceId = "123",// swaas id
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "123aaa",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));
        legacyProvider.DeleteVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitchLink_Error()
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest
        {
            Id = "123aaa",  // vsl id
            ResourceId = "123",// swaas id
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "123aaa",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));
        legacyProvider.DeleteVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput { Success = false });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }
    
    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitchLink_NotFound()
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest
        {
            Id = "xxxxxxxx",  // vsl id
            ResourceId = "123",// swaas id
            UserId = UserId,
            ProjectId = "default",
        };

        var legacyVirtualSwitchLink = new LegacyVirtualSwitchLink
        {
            Resource = new LegacyLink
            {
                ResourceId = "123aaa",
                ServiceType = Abstractions.Providers.Models.Legacy.Internal.LegacyServiceType.Server,
                ResourceName = "server1"
            },
            VirtualNetwork = new LegacyVirtualSwitch
            {
                VirtualNetworkId = "123a",
                FriendlyName = "vswitch-bergamo",
                Status = VirtualSwitchStatuses.Active,
                Region = "IT1-IT2"
            }
        };

        var legacyVirtualSwitchLinks = new List<LegacyVirtualSwitchLink> { legacyVirtualSwitchLink };

        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>(legacyVirtualSwitchLinks));
        legacyProvider.DeleteVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitchLink_LegacyError()
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest
        {
            Id = "123aaa",  // vsl id
            ResourceId = "123",// swaas id
            UserId = UserId,
            ProjectId = "default",
        };


        var provider = CreateServiceCollection().BuildServiceProvider();
        var legacyProvider = provider.GetRequiredService<ISwaasesProvider>();

        legacyProvider.GetVirtualSwitchLinks(It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput<List<LegacyVirtualSwitchLink>>() { Success =false});
        legacyProvider.DeleteVirtualSwitchLink(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).ReturnsForAnyArgs(new ApiCallOutput { Success = true });

        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task DeleteVirtualSwitchLink_NoUserId()
    {
        var request = new VirtualSwitchLinkDeleteUseCaseRequest
        {
            Id = "123aaa",  // vsl id
            ResourceId = "123",// swaas id
            UserId = null,
            ProjectId = "default",
        };

        var provider = CreateServiceCollection().BuildServiceProvider();
        
        var service = provider.GetRequiredService<ISwaasesService>();

        var result = await service.DeleteVirtualSwitchLink(request, default);

        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region "Utils"

    private LegacySwaasListItem legacySwaasListItem = new LegacySwaasListItem()
    {
        Id = 123,
        Status = SwaasStatuses.Active,
        Name = "swaas-name",
    };

    private LegacySwaasDetail LegacySwaas = new LegacySwaasDetail()
    {
        Id = 123,
        Status = SwaasStatuses.Active,
        Name = "swaas-name"
    };

    #endregion
}
