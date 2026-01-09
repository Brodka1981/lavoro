using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using FluentAssertions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;
public class SwaasMappingTest : TestBase
{
    public SwaasMappingTest(ITestOutputHelper output) : base(output)
    {
    }

    #region Response
    [Fact]
    [Unit]
    public void SwaaS_Success_NotNull()
    {
        var swaas = CreateSwaas();
        swaas.Properties = new SwaasProperties()
        {
            ActivationDate = DateTimeOffset.Now,
            AutoRenewEnabled = true,
            DueDate = DateTimeOffset.Now,
            Folders = new List<string>() { "Folder1" },
            MonthlyUnitPrice = 2,
            RenewAllowed = false,
            UpgradeAllowed = false,
            Admin = "",
            Model = "",
            Reply = false
        };
        swaas.UpdatedBy = "AM";

        var mapped = swaas.MapToResponse();

        CommonMappingTests.ValidateBaseResource<Swaas, SwaasResponseDto, SwaasPropertiesResponseDto>(swaas, mapped!, swaas.UpdatedBy);
        mapped?.Properties?.Should().NotBeNull();
        mapped?.Properties?.DueDate.Should().Be(swaas.Properties.DueDate);
        mapped?.Properties?.ActivationDate.Should().Be(swaas.Properties.ActivationDate);
        mapped?.Properties?.MonthlyUnitPrice.Should().Be(swaas.Properties.MonthlyUnitPrice);
        mapped?.Properties?.AutoRenewEnabled.Should().Be(swaas.Properties.AutoRenewEnabled);
        mapped?.Properties?.RenewAllowed.Should().Be(swaas.Properties.RenewAllowed);
        mapped?.Properties?.UpgradeAllowed.Should().Be(swaas.Properties.UpgradeAllowed);
        mapped?.Properties?.Folders.Should().NotBeNull().And.HaveCount(swaas.Properties.Folders.Count()).And.HaveElementAt(0, swaas.Properties.Folders.FirstOrDefault());
        mapped?.Properties?.Admin.Should().Be(swaas.Properties.Admin);
        mapped?.Properties?.Reply.Should().Be(swaas.Properties.Reply);
    }

    #endregion
    private static Swaas CreateSwaas()
    {
        var ret = CommonMappingTests.CreateBaseResource<Swaas>();
        ret.Properties = new SwaasProperties();
        return ret;
    }
}
