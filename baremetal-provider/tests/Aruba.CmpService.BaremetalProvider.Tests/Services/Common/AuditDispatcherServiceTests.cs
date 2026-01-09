using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
//using Aruba.CmpService.Libraries.Audit.Enums;
//using Aruba.CmpService.Libraries.Audit.Models;
//using Aruba.CmpService.Libraries.Audit.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services;

public class AuditDispatcherServiceTests : TestBase
{

    public AuditDispatcherServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        //var auditService = Substitute.For<IAuditService>();

        //auditService.CreateAudit(It.IsAny<string>(), It.IsAny<AuditStatus>(), It.IsAny<AuditLevel>(), It.IsAny<AuditChannel>())
        //    .ReturnsForAnyArgs(new Auditing());

        //services.AddSingleton(auditService);

        var actionContextAccessor = Substitute.For<IActionContextAccessor>();
        services.AddSingleton(actionContextAccessor);

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        services.AddSingleton(httpContextAccessor);

        var profileProvider = Substitute.For<IProfileProvider>();
        profileProvider.GetUser(It.IsAny<string>())
            .ReturnsForAnyArgs(new ApiCallOutput<UserProfile>(new UserProfile()
            {
                Company = Guid.NewGuid().ToString(),
                Tenant = Guid.NewGuid().ToString(),
            }));

        services.AddSingleton(profileProvider);

    }

    [Fact]
    [Unit]
    public void GetFailedEnvelope()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        //var auditService = provider.GetRequiredService<IAuditService>();

        //auditService.CreateAudit(It.IsAny<string>(), It.IsAny<AuditStatus>(), It.IsAny<AuditLevel>(), It.IsAny<AuditChannel>())
        //    .ReturnsForAnyArgs(new Auditing());

        var server = new Server()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            CreatedBy = "aru-24468",
            Location = new Abstractions.Models.Location()
            {
                Value = "Bergamo"
            },
            Category = new Abstractions.Models.Category()
            {
                Name = Abstractions.Constants.Categories.BaremetalServer.Value,
                Typology = new Abstractions.Models.Typology()
                {
                    Id = Typologies.Server.Value
                }
            },
            Project = new Abstractions.Models.Project()
            {
                Id = Guid.NewGuid().ToString()
            }
        };
    }

    [Fact]
    [Unit]
    public void GetSuccessEnvelope()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        //var auditService = provider.GetRequiredService<IAuditService>();

        //auditService.CreateAudit(It.IsAny<string>(), It.IsAny<AuditStatus>(), It.IsAny<AuditLevel>(), It.IsAny<AuditChannel>())
        //    .ReturnsForAnyArgs(new Auditing());

        var server = new Server()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "test",
            CreatedBy = "aru-24468",
            Location = new Abstractions.Models.Location()
            {
                Value = "Bergamo"
            },
            Category = new Abstractions.Models.Category()
            {
                Name = Abstractions.Constants.Categories.BaremetalServer.Value,
                Typology = new Abstractions.Models.Typology()
                {
                    Id = Typologies.Server.Value
                }
            },
            Project = new Abstractions.Models.Project()
            {
                Id = Guid.NewGuid().ToString()
            }
        };

    }
}
