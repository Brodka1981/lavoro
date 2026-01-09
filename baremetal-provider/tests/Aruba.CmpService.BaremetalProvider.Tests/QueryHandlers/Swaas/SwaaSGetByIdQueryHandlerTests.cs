using System.Collections.ObjectModel;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS.Wrappers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS;
public class SwaaSGetByIdQueryHandlerTests : TestBase
{
    public SwaaSGetByIdQueryHandlerTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<SwaasGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var swaasService = Substitute.For<ISwaasesService>();
        services.AddSingleton(swaasService);

        services.AddSingleton<SwaaSGetByIdQueryHandlerWrapper>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetServerById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var swaasService = provider.GetRequiredService<ISwaasesService>();
        swaasService.GetById(It.IsAny<SwaasByIdRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<Swaas>()
            {
                Value = new Swaas
                {
                    Id = "123",
                    CreatedBy = "aru-24468",
                    Location = new Location()
                    {
                        Value = "ITBG"
                    },
                    Properties = new SwaasProperties()
                }
            });

        var wrapper = provider.GetRequiredService<SwaaSGetByIdQueryHandlerWrapper>();
        var request = new SwaasByIdRequest()
        {
            ResourceId = "123"
        };

        var swaasResponse = await wrapper.Handle(request);

        swaasResponse.Should().NotBeNull();

        swaasResponse!.Id.Should().Be("123");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetServerById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var serversService = provider.GetRequiredService<ISwaasesService>();
        serversService.GetById(It.IsAny<SwaasByIdRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<Swaas>()
            {
                Errors = new Collection<IServiceResultError>() { new NotFoundError("123") }
            });

        var wrapper = provider.GetRequiredService<SwaaSGetByIdQueryHandlerWrapper>();
        var request = new SwaasByIdRequest()
        {
            ResourceId = "123"
        };

        var serverResponse = await wrapper.Handle(request);

        serverResponse.Should().BeNull();
    }
}
