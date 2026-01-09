using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class PaymentsQueryHandlersTests : TestBase
{
    public PaymentsQueryHandlersTests(ITestOutputHelper output)
        : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<FirewallGetByIdQueryHandler>>();
        services.AddSingleton(logger);

        var paymentsService = Substitute.For<IPaymentsService>();
        services.AddSingleton(paymentsService);

        var paymentsProvider = Substitute.For<IPaymentsProvider>();
        services.AddSingleton(paymentsProvider);

        services.AddSingleton<PaymentMethodGetByIdQueryHandler>();
        services.AddSingleton<PaymentMethodGetAllQueryHandler>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetPaymentlById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        paymentsService.GetPaymentMethodByIdAsync(new PaymentMethodByIdRequest() { Id = "111" }, CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<PaymentMethod>()
            {
                Value = new PaymentMethod
                {
                    Id = "123",
                    Name = "Name",
                    Type = LegacyPaymentType.PayPal,
                    Value = "***"
                }
            });

        var queryHandler = provider.GetRequiredService<PaymentMethodGetByIdQueryHandler>();
        var request = new PaymentMethodByIdRequest()
        {
            Id = "123",
        };

        var response = await queryHandler.Handle(request);

        response.Should().NotBeNull();

        response.Id.Should().Be("123");
        response.Name.Should().Be("Name");
        response.Type.Should().Be(LegacyPaymentType.PayPal);
        response.Value.Should().Be("***");
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_GetPaymentById_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        paymentsService.GetPaymentMethodByIdAsync(It.IsAny<PaymentMethodByIdRequest>(), CancellationToken.None)
           .ReturnsForAnyArgs(ServiceResult<PaymentMethod>.CreateNotFound("1"));


        var queryHandler = provider.GetRequiredService<PaymentMethodGetByIdQueryHandler>();
        var request = new PaymentMethodByIdRequest()
        {
            Id = "121"
        };

        var response = await queryHandler.Handle(request);
        response.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchPayments_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var paymentsService = provider.GetRequiredService<IPaymentsService>();

        var payments = Enumerable.Range(0, 4).Select(s => new PaymentMethod()
        {
            Id = s.ToString(),
            Type = (LegacyPaymentType)s,
            Name = ((LegacyPaymentType)s).ToString(),
            Value = $"Name_{((LegacyPaymentType)s)}",
        }).ToList();


        paymentsService.GetPaymentMethodsAsync(It.IsAny<PaymentMethodsAllRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(new ServiceResult<IEnumerable<PaymentMethod>>()
            {
                Value = payments
            }
);

        var queryHandler = provider.GetRequiredService<PaymentMethodGetAllQueryHandler>();
        var request = new PaymentMethodsAllRequest();

        var response = await queryHandler.Handle(request);
        response.Should().HaveCount(4);
        for (var i = 0; i < 4; i++)
        {
            response.ElementAt(i).Should().NotBeNull();
            response.ElementAt(i).Id.Should().Be(i.ToString(CultureInfo.InvariantCulture));
            response.ElementAt(i).Type.Should().Be((LegacyPaymentType)i);
            response.ElementAt(i).Name.Should().Be(((LegacyPaymentType)i).ToString());
            response.ElementAt(i).Value.Should().Be($"Name_{((LegacyPaymentType)i)}");
        }
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var paymentsService = provider.GetRequiredService<IPaymentsService>();

        paymentsService.GetPaymentMethodsAsync(It.IsAny<PaymentMethodsAllRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<IEnumerable<PaymentMethod>>.CreateNotFound("a"));

        var queryHandler = provider.GetRequiredService<PaymentMethodGetAllQueryHandler>();
        var request = new PaymentMethodsAllRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_ForbiddenError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var paymentsService = provider.GetRequiredService<IPaymentsService>();

        paymentsService.GetPaymentMethodsAsync(It.IsAny<PaymentMethodsAllRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<IEnumerable<PaymentMethod>>.CreateForbiddenError());

        var queryHandler = provider.GetRequiredService<PaymentMethodGetAllQueryHandler>();
        var request = new PaymentMethodsAllRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SearchFirewall_InternalServerError()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var paymentsService = provider.GetRequiredService<IPaymentsService>();

        paymentsService.GetPaymentMethodsAsync(It.IsAny<PaymentMethodsAllRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(ServiceResult<IEnumerable<PaymentMethod>>.CreateInternalServerError());

        var queryHandler = provider.GetRequiredService<PaymentMethodGetAllQueryHandler>();
        var request = new PaymentMethodsAllRequest();

        var firewallResponse = await queryHandler.Handle(request);

        firewallResponse.Should().BeNull();
    }
}
