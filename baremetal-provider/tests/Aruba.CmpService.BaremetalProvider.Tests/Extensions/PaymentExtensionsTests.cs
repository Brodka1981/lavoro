using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Extensions;
public class PaymentExtensionsTests :
    TestBase
{
    public PaymentExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public void Dictionary_AddRange()
    {
        var info = new LegacyAutoRenewInfo()
        {
            DeviceId = 1,
            DeviceType = LegacyPaymentType.CreditCard
        };
        var ret = info.CreateCmpDeviceId();
        ret.Should().NotBeNull();
        ret.Should().Be("1001");
    }
}
