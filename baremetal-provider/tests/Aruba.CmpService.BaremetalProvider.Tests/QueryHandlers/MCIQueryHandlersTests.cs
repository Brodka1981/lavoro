using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Aruba.MessageBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class MCIQueryHandlersTests : TestBase
{
    public MCIQueryHandlersTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<MCIGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var mcisService = Substitute.For<IMCIsService>();
        services.AddSingleton(mcisService);

        var projectProvider = Substitute.For<IProjectProvider>();
        services.AddSingleton(projectProvider);

        services.AddSingleton<MCIGetByIdQueryHandler>();
        services.AddSingleton<MCISearchQueryHandler>();

        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<UserProfile>(new UserProfile
            {
                IsResellerCustomer = true
            }));

        services.AddSingleton(profileProvider);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetMCIById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var mcisService = provider.GetRequiredService<IMCIsService>();
        mcisService.GetById(new MCIByIdRequest() { ResourceId = "123" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<MCI>()
            {
                Value = new MCI
                {
                    Id = "123",
                    CreatedBy = "ARU-25198",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new MCIProperties()
                    
                }
            });

        var queryHandler = provider.GetRequiredService<MCIGetByIdQueryHandler>();
        var request = new MCIByIdRequest()
        {
            ResourceId = "123"
        };

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetMCIById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var mcisService = provider.GetRequiredService<IMCIsService>();
        mcisService.GetById(It.IsAny<MCIByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<MCI>.CreateNotFound("1"));


        var queryHandler = provider.GetRequiredService<MCIGetByIdQueryHandler>();
        var request = new MCIByIdRequest()
        {
            ResourceId = "121"
        };

        var response = await queryHandler.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var mcisService = provider.GetRequiredService<IMCIsService>();

        mcisService.Search(It.IsAny<MCISearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<MCIList>()
            {
                Value = new MCIList
                {
                    Values = new List<MCI>()
                    {
                        new MCI()
                        {
                           Id = "123",
                           CreatedBy = "ARU-25198",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new MCIProperties()
                        }
                    }
                }
            });

        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest();

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Values.Count.Should().Be(1);
        mciResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_ProjectResponse_Error()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IMCIsService), typeof(MCIsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IMCIsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IMCICatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>() { Success = false });


        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_ProjectResponse_Result_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IMCIsService), typeof(MCIsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IMCIsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IMCICatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true
            });


        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_ProjectResponse_Result_Properties_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IMCIsService), typeof(MCIsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IMCIsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IMCICatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
            });


        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_ProjectResponse_Result_NonDefault()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IMCIsService), typeof(MCIsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IMCIsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IMCICatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
                {
                    Properties = new Abstractions.Providers.Models.Projects.ProjectProperties()
                }
            });


        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_ProjectResponse_Result_Default()
    {
        var mciProvider = Substitute.For<IMCIsProvider>();
        mciProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<LegacyListResponse<LegacyMCIListItem>>()
            {
                Success = true,
                Result = new LegacyListResponse<LegacyMCIListItem>()
            });


        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IMCIsService), typeof(MCIsService), ServiceLifetime.Singleton))
            .AddSingleton(mciProvider)
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IMCICatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
                {
                    Properties = new Abstractions.Providers.Models.Projects.ProjectProperties()
                    {
                        Default = true
                    }
                }
            });



        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var mciResponse = await queryHandler.Handle(request);
        mciResponse.Should().NotBeNull();
        mciResponse.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var mcisService = provider.GetRequiredService<IMCIsService>();

        mcisService.Search(It.IsAny<MCISearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<MCIList>.CreateNotFound("a"));

        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest();

        var mciResponse = await queryHandler.Handle(request);

        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var mciService = provider.GetRequiredService<IMCIsService>();

        mciService.Search(It.IsAny<MCISearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<MCIList>.CreateForbiddenError());

        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest();

        var mciResponse = await queryHandler.Handle(request);

        mciResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchMCI_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var mcisService = provider.GetRequiredService<IMCIsService>();

        mcisService.Search(It.IsAny<MCISearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<MCIList>.CreateInternalServerError());

        var queryHandler = provider.GetRequiredService<MCISearchQueryHandler>();
        var request = new MCISearchFilterRequest();

        var mciResponse = await queryHandler.Handle(request);

        mciResponse.Should().BeNull();
    }
}
