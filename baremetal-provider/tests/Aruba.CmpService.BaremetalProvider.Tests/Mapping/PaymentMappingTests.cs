using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;

public class PaymentMappingTests : TestBase
{
    public PaymentMappingTests(ITestOutputHelper output)
        : base(output)
    { }

    #region Response
    [Fact]
    [Unit]
    public void Payments_Success_WithItems()
    {
        var payments = Enumerable.Range(0, 4).Select(s => new PaymentMethod()
        {
            Id = s.ToString(),
            Type = (LegacyPaymentType)s,
            Name = ((LegacyPaymentType)s).ToString(),
            Value = $"Name_{((LegacyPaymentType)s)}",
        });
        var mapped = payments.MapToResponse();


        mapped.Should().NotBeNull();
        mapped.Should().HaveCount(4);
        for (var i = 0; i < 4; i++)
        {
            mapped[i].Should().NotBeNull();
            mapped[i].Id.Should().Be(i.ToString(CultureInfo.InvariantCulture));
            mapped[i].Type.Should().Be((LegacyPaymentType)i);
            mapped[i].Name.Should().Be(((LegacyPaymentType)i).ToString());
            mapped[i].Value.Should().Be($"Name_{((LegacyPaymentType)i)}");
        }
    }

    [Fact]
    [Unit]
    public void Payments_Success_EmptyItems()
    {
        var payments = Enumerable.Range(0, 0).Select(s => new PaymentMethod()
        {
            Id = s.ToString(),
            Type = (LegacyPaymentType)s,
            Name = ((LegacyPaymentType)s).ToString(),
            Value = $"Name_{((LegacyPaymentType)s)}",
        });
        var mapped = payments.MapToResponse();


        mapped.Should().NotBeNull();
        mapped.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void Payments_Success_Null()
    {
        IEnumerable<PaymentMethod> payments = null;
        var mapped = payments.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public void Payment_Success()
    {
        var payment = new PaymentMethod()
        {
            Id = "11",
            Type = LegacyPaymentType.Ingenico,
            Name = "Ingenico",
            Value = "Name_Ingenico",
        };
        var mapped = payment.MapToResponse();


        mapped.Should().NotBeNull();
        mapped.Id.Should().Be("11");
        mapped.Type.Should().Be(LegacyPaymentType.Ingenico);
        mapped.Name.Should().Be("Ingenico");
        mapped.Value.Should().Be("Name_Ingenico");
    }

    [Fact]
    [Unit]
    public void Payment_Success_Null()
    {
        PaymentMethod payment = null;
        var mapped = payment.MapToResponse();

        mapped.Should().BeNull();
    }
    #endregion

    #region Model
    #endregion
}

