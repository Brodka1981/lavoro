using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Aruba.MessageBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class HPCQueryHandlersTests : TestBase
{
    public HPCQueryHandlersTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<HPCGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var hpcsService = Substitute.For<IHPCsService>();
        services.AddSingleton(hpcsService);

        var projectProvider = Substitute.For<IProjectProvider>();
        services.AddSingleton(projectProvider);

        services.AddSingleton<HPCGetByIdQueryHandler>();
        services.AddSingleton<HPCSearchQueryHandler>();

        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<UserProfile>(new UserProfile
            {
                IsResellerCustomer = true
            }));

        services.AddSingleton(profileProvider);

        var messagebus = Substitute.For<IMessageBus>();
        services.AddSingleton(messagebus);

        var paymentService = Substitute.For<IPaymentsService>();
        services.AddSingleton(paymentService);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetHPCById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcsService = provider.GetRequiredService<IHPCsService>();
        hpcsService.GetById(new HPCByIdRequest() { ResourceId = "123" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<HPC>()
            {
                Value = new HPC
                {
                    Id = "123",
                    CreatedBy = "ARU-25198",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new HPCProperties()

                }
            });

        var queryHandler = provider.GetRequiredService<HPCGetByIdQueryHandler>();
        var request = new HPCByIdRequest()
        {
            ResourceId = "123"
        };

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetHPCById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var hpcsService = provider.GetRequiredService<IHPCsService>();
        hpcsService.GetById(It.IsAny<HPCByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<HPC>.CreateNotFound("1"));


        var queryHandler = provider.GetRequiredService<HPCGetByIdQueryHandler>();
        var request = new HPCByIdRequest()
        {
            ResourceId = "121"
        };

        var response = await queryHandler.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var hpcsService = provider.GetRequiredService<IHPCsService>();

        hpcsService.Search(It.IsAny<HPCSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<HPCList>()
            {
                Value = new HPCList
                {
                    Values = new List<HPC>()
                    {
                        new HPC()
                        {
                           Id = "123",
                           CreatedBy = "ARU-25198",
                           Location = new Location()
                           {
                               Value = "ITBG"
                           },
                           Properties = new HPCProperties()
                        }
                    }
                }
            });

        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest();

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Values.Count.Should().Be(1);
        hpcResponse.Values.First().Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_ProjectResponse_Error()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IHPCsService), typeof(HPCsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IHPCsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IHPCCatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>() { Success = false });


        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_ProjectResponse_Result_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IHPCsService), typeof(HPCsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IHPCsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IHPCCatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true
            });


        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_ProjectResponse_Result_Properties_Null()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IHPCsService), typeof(HPCsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IHPCsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IHPCCatalogRepository>())
            .BuildServiceProvider();
        var projectProvider = provider.GetRequiredService<IProjectProvider>();
        projectProvider.GetProjectAsync(It.IsAny<string>(), It.IsAny<string>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<Abstractions.Providers.Models.Projects.Project>()
            {
                Success = true,
                Result = new Abstractions.Providers.Models.Projects.Project()
            });


        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_ProjectResponse_Result_NonDefault()
    {
        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IHPCsService), typeof(HPCsService), ServiceLifetime.Singleton))
            .AddSingleton(Substitute.For<IHPCsProvider>())
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IHPCCatalogRepository>())
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


        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_ProjectResponse_Result_Default()
    {
        var hpcProvider = Substitute.For<IHPCsProvider>();
        hpcProvider.Search(It.IsAny<LegacySearchFilters>())
            .ReturnsForAnyArgs(new Abstractions.Providers.Models.ApiCallOutput<LegacyListResponse<LegacyHPCListItem>>()
            {
                Success = true,
                Result = new LegacyListResponse<LegacyHPCListItem>()
            });


        var provider = CreateServiceCollection()
            .Replace(new ServiceDescriptor(typeof(IHPCsService), typeof(HPCsService), ServiceLifetime.Singleton))
            .AddSingleton(hpcProvider)
            .AddSingleton(Substitute.For<ILocationMapRepository>())
            .AddSingleton(Substitute.For<ICatalogueProvider>())
            .AddSingleton(Substitute.For<IPaymentsProvider>())
            .AddSingleton(Substitute.For<IProfileProvider>())
            .AddSingleton(Substitute.For<IMessageBus>())
            .AddSingleton(Substitute.For<IHPCCatalogRepository>())
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



        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest()
        {
            UserId = "userid",
            ProjectId = "ProjectId"
        };

        var hpcResponse = await queryHandler.Handle(request);
        hpcResponse.Should().NotBeNull();
        hpcResponse.Values.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var hpcsService = provider.GetRequiredService<IHPCsService>();

        hpcsService.Search(It.IsAny<HPCSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<HPCList>.CreateNotFound("a"));

        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest();

        var hpcResponse = await queryHandler.Handle(request);

        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_Forbidden()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var hpcService = provider.GetRequiredService<IHPCsService>();

        hpcService.Search(It.IsAny<HPCSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<HPCList>.CreateForbiddenError());

        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest();

        var hpcResponse = await queryHandler.Handle(request);

        hpcResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchHPC_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var hpcsService = provider.GetRequiredService<IHPCsService>();

        hpcsService.Search(It.IsAny<HPCSearchFilterRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<HPCList>.CreateInternalServerError());

        var queryHandler = provider.GetRequiredService<HPCSearchQueryHandler>();
        var request = new HPCSearchFilterRequest();

        var hpcResponse = await queryHandler.Handle(request);

        hpcResponse.Should().BeNull();
    }
}
