using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services.Common;

public class PaymentServiceTests : TestBase
{
    private const string UserId = "aru-25085";
    private const string ProjectId = "48965f1e53wer";

    public PaymentServiceTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var logger = Substitute.For<ILogger<PaymentsService>>();
        services.AddSingleton(logger);

        var profileProvider = Substitute.For<IProfileProvider>();
        services.AddSingleton(profileProvider);

        var paymentsProvider = Substitute.For<IPaymentsProvider>();
        services.AddSingleton(paymentsProvider);
        services.AddSingleton<IPaymentsService, PaymentsService>();
    }

    #region All Payments
    [Fact]
    [Unit]
    public async Task All_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(4);
        result.Value.ElementAt(0).Type.Should().Be(LegacyPaymentType.CreditCard);
        result.Value.ElementAt(1).Type.Should().Be(LegacyPaymentType.PayPal);
        result.Value.ElementAt(2).Type.Should().Be(LegacyPaymentType.Ingenico);
        result.Value.ElementAt(3).Type.Should().Be(LegacyPaymentType.Sdd);

    }

    [Fact]
    [Unit]
    public async Task All_IsFraud_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(true));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(0);

    }

    [Fact]
    [Unit]
    public async Task All_IsFraud_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false });


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();

    }

    [Fact]
    [Unit]
    public async Task All_LegacyResponse_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>() { Success = false });
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task All_LegacyResponse_NullResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(null));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }
    #endregion

    #region Payment by Id
    [Fact]
    [Unit]
    public async Task ById_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodByIdRequest()
        {
            UserId = "1234",
            Id = "1001"
        };
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Type.Should().Be(LegacyPaymentType.CreditCard);
    }
    [Fact]
    [Unit]
    public async Task ById_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodByIdRequest()
        {
            UserId = "1234",
            Id = "10aaa01"
        };
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ById_IsFraud_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(true));

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodByIdRequest()
        {
            UserId = "1234",
            Id = "1"
        };
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task ById_IsFraud_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(this.GetPayments()));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>() { Success = false });

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodByIdRequest()
        {
            UserId = "1234",
            Id = "1"
        };
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }

    [Fact]
    [Unit]
    public async Task ById_LegacyResponse_Error()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>() { Success = false });
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));

        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodByIdRequest()
        {
            UserId = "1234",
            Id = "1"
        };
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();

    }

    [Fact]
    [Unit]
    public async Task ById_LegacyResponse_NullResult()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var paymentsProvider = provider.GetRequiredService<IPaymentsProvider>();

        paymentsProvider.GetPaymentMethodsAsync().ReturnsForAnyArgs(new ApiCallOutput<IEnumerable<LegacyPaymentMethod>>(null));
        paymentsProvider.GetFraudRiskAssessmentAsync().ReturnsForAnyArgs(new ApiCallOutput<bool>(false));


        var paymentsService = provider.GetRequiredService<IPaymentsService>();
        var request = new PaymentMethodsAllRequest()
        {
            UserId = "1234",
        };
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        result.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().BeOfType<FailureError>();
    }
    #endregion

    private IEnumerable<LegacyPaymentMethod> GetPayments()
    {
        return new List<LegacyPaymentMethod>()
        {
            new LegacyPaymentMethod()
            {
                DeviceType = LegacyPaymentType.CreditCard,
                Id = 1,
                Name = "Credit Card",
                Status = LegacyPaymentStatus.Active,
                TokenMasked = "hjhjhjdfds"
            },
            new LegacyPaymentMethod()
            {
                DeviceType = LegacyPaymentType.PayPal,
                Id = 2,
                Name = "Paypal",
                Status = LegacyPaymentStatus.Active,
                TokenMasked = "hjhjhjdfds"
            },
            new LegacyPaymentMethod()
            {
                DeviceType = LegacyPaymentType.Ingenico,
                Id = 3,
                Name = "Ingenico",
                Status = LegacyPaymentStatus.Active,
                TokenMasked = "hjhjhjdfds"
            },
            new LegacyPaymentMethod()
            {
                DeviceType = LegacyPaymentType.Sdd,
                Id = 3,
                Name = "SDD",
                Status = LegacyPaymentStatus.Active,
                TokenMasked = "hjhjhjdfds"
            }
        };
    }
}
