using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Various;
public class LegacyLabelsErrorsTests :
    TestBase
{
    public LegacyLabelsErrorsTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Unit]
    public void LegacyError_Null()
    {
        var legacyError = LegacyLabelErrors.Create("Invalid", Typologies.Server);
        legacyError.Should().BeNull();

    }

    [Fact]
    [Unit]
    public void LegacyError_NullErrorCode()
    {
        var legacyError = LegacyLabelErrors.Create(null, Typologies.Server);
        legacyError.Should().BeNull();

    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("The resource name length must be be between 4 and 50 characters");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SERVER_REBOOT_MANAGED()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SERVER_REBOOT_MANAGED", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("A managed server can't be rebooted");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SERVER_REBOOT_MANAGED");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SERVER_REBOOT_WEBMATRIX()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SERVER_REBOOT_WEBMATRIX", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("A webmatrix server can't be rebooted");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SERVER_REBOOT_WEBMATRIX");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SERVER_REBOOT_OFFLINE()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SERVER_REBOOT_OFFLINE", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("The resource is offline");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SERVER_REBOOT_OFFLINE");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SERVER_REBOOT_REBOOT_IN_PROGRESS()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SERVER_REBOOT_REBOOT_IN_PROGRESS", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("The resource is rebooting");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SERVER_REBOOT_REBOOT_IN_PROGRESS");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_SERVER_REBOOT_SERVICE_EXPIRED()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_SERVER_REBOOT_SERVICE_EXPIRED", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("The resource is expired");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_SERVER_REBOOT_SERVICE_EXPIRED");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_CLOUDDCS_INVALID_AUTORENEW_INPUT_DATA()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_CLOUDDCS_INVALID_AUTORENEW_INPUT_DATA", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("Invalid autorenew data");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_CLOUDDCS_INVALID_AUTORENEW_INPUT_DATA");
    }

    [Fact]
    [Unit]
    public void LegacyError_VLD_PAYMENT_PUTAUTOMATICRENEWAL_EXPIRE_DATE_OUT_OF_RANGE()
    {
        var legacyError = LegacyLabelErrors.Create("VLD_PAYMENT_PUTAUTOMATICRENEWAL_EXPIRE_DATE_OUT_OF_RANGE", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("The resource is nearing expiration");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.VLD_PAYMENT_PUTAUTOMATICRENEWAL_EXPIRE_DATE_OUT_OF_RANGE");
    }

    [Fact]
    [Unit]
    public void LegacyError_ERR_PAYMENT_PUTAUTOMATICRENEWAL_FAILED()
    {
        var legacyError = LegacyLabelErrors.Create("ERR_PAYMENT_PUTAUTOMATICRENEWAL_FAILED", Typologies.Server);
        legacyError.Should().NotBeNull();
        legacyError.Message.Should().Be("Invalid autorenew data");
        legacyError.Label.Should().Be("PROVISIONING_FILE.PROVISIONING.SERVER.LEGACY.INPUT.ERRORS_MESSAGE.ERR_PAYMENT_PUTAUTOMATICRENEWAL_FAILED");
    }
}
