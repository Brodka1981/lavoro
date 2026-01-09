using System.Linq;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;

public class InternalMappingTests : TestBase
{
    public InternalMappingTests(ITestOutputHelper output)
        : base(output)
    {
    }

    #region Response

    [Fact]
    [Unit]
    public void LegacyResource_MapToResponse_Success()
    {
        var resource = new Abstractions.Models.Internal.LegacyResource()
        {
            BillingPeriod = "Month",
            Id = 123,
            MonthlyUnitPrice = 99.99m,
            AutoRenewDeviceId = "123005",
            AutoRenewEnabled = true,
            Name = "Test-Resource",
            Uri = "https://example.com/resource",
            DueDate = DateTimeOffset.Now.AddMonths(1),
            AutoRenewMonths = 12,
            Status = "Active",
            TypologyId = "server"
        };

        var mapped = resource.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.BillingPeriod.Should().Be(resource.BillingPeriod);
        mapped.Id.Should().Be(resource.Id);
        mapped.MonthlyUnitPrice.Should().Be(resource.MonthlyUnitPrice);
        mapped.AutoRenewDeviceId.Should().Be(resource.AutoRenewDeviceId);
        mapped.AutoRenewEnabled.Should().Be(resource.AutoRenewEnabled);
        mapped.Name.Should().Be(resource.Name);
        mapped.Uri.Should().Be(resource.Uri);
        mapped.DueDate.Should().Be(resource.DueDate);
        mapped.AutoRenewMonths.Should().Be(resource.AutoRenewMonths);
        mapped.ShowVat.Should().BeFalse(); // valore fisso nel mapping
        mapped.Status.Should().Be(resource.Status);
        mapped.TypologyId.Should().Be(resource.TypologyId);
    }

    [Fact]
    [Unit]
    public void BaseLegacyResource_MapToResponse_Success()
    {
        var resource = new BaseLegacyResource()
        {
            BillingPeriod = "Month",
            Id = 456,
            MonthlyUnitPrice = 49.99m,
            AutoRenewDeviceId = "123005",
            AutoRenewEnabled = false,
            Name = "Base-Resource",
            DueDate = DateTimeOffset.Now.AddDays(15),
            AutoRenewMonths = 6,
            Status = "Active",
            TypologyId = "database"
        };

        var mapped = resource.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.BillingPeriod.Should().Be(resource.BillingPeriod);
        mapped.Id.Should().Be(resource.Id);
        mapped.MonthlyUnitPrice.Should().Be(resource.MonthlyUnitPrice);
        mapped.AutoRenewDeviceId.Should().Be(resource.AutoRenewDeviceId);
        mapped.AutoRenewEnabled.Should().Be(resource.AutoRenewEnabled);
        mapped.Name.Should().Be(resource.Name);
        mapped.DueDate.Should().Be(resource.DueDate);
        mapped.AutoRenewMonths.Should().Be(resource.AutoRenewMonths);
        mapped.Status.Should().Be(resource.Status);
        mapped.TypologyId.Should().Be(resource.TypologyId);
    }

    [Fact]
    [Unit]
    public void BaseLegacyResource_List_MapToResponse_Success()
    {
        var resources = new List<BaseLegacyResource>()
        {
            new BaseLegacyResource()
            {
                BillingPeriod = "Month",
                Id = 1,
                MonthlyUnitPrice = 10.0m,
                AutoRenewDeviceId = "123005",
                AutoRenewEnabled = true,
                Name = "Resource",
                DueDate = DateTimeOffset.Now.AddDays(10),
                AutoRenewMonths = 3,
                Status = "Active",
                TypologyId = "server"
            },
            new BaseLegacyResource()
            {
                BillingPeriod = "Month2",
                Id = 2,
                MonthlyUnitPrice = 20.0m,
                AutoRenewDeviceId = "123005",
                AutoRenewEnabled = false,
                Name = "Resource-2",
                DueDate = DateTimeOffset.Now.AddDays(20),
                AutoRenewMonths = 6,
                Status = "Active",
                TypologyId = "server"
            }
        };

        var mappedList = resources.MapToResponse().ToList();

        mappedList.Should().NotBeNull();
        mappedList.Should().HaveCount(2);

        for (int i = 0; i < resources.Count; i++)
        {
            mappedList[i].BillingPeriod.Should().Be(resources[i].BillingPeriod);
            mappedList[i].Id.Should().Be(resources[i].Id);
            mappedList[i].MonthlyUnitPrice.Should().Be(resources[i].MonthlyUnitPrice);
            mappedList[i].AutoRenewDeviceId.Should().Be(resources[i].AutoRenewDeviceId);
            mappedList[i].AutoRenewEnabled.Should().Be(resources[i].AutoRenewEnabled);
            mappedList[i].Name.Should().Be(resources[i].Name);
            mappedList[i].DueDate.Should().Be(resources[i].DueDate);
            mappedList[i].AutoRenewMonths.Should().Be(resources[i].AutoRenewMonths);
            mappedList[i].Status.Should().Be(resources[i].Status);
            mappedList[i].TypologyId.Should().Be(resources[i].TypologyId);
        }
    }

    [Fact]
    [Unit]
    public void LegacyAutorecharge_MapToResponse_Success()
    {
        var legacyAutorecharge = new LegacyAutorechargeData()
        {
            Enabled = true,
            DeviceId = 123005,
            DeviceType = LegacyPaymentType.Wallet,
            CreditToAutoRecharge = 25.50m
        };

        var mapped = legacyAutorecharge.MapToResponse();

        mapped.Should().NotBeNull();
        mapped.Enabled.Should().Be(legacyAutorecharge.Enabled);
        mapped.DeviceId.Should().Be(legacyAutorecharge.DeviceId);
        mapped.DeviceType.Should().Be(legacyAutorecharge.DeviceType);
        mapped.CreditToAutoRecharge.Should().Be(legacyAutorecharge.CreditToAutoRecharge);
    }

    #endregion

    #region ToModel
    [Fact]
    [Unit]
    public void LegacyResourceProvider_MapToLegacyResource_Success()
    {
        var providerResource = new Abstractions.Providers.Models.Legacy.Internal.LegacyResource()
        {
            Id = 100,
            Name = "Legacy Provider Resource",
            Price = 19.99m,
            ExpiringDate = DateTime.Now.AddDays(30),
            AutoRenewFrequency = 3,
            Status = "Active",
            ServiceType = LegacyServiceType.Server,
            AutoRenewEnabled = true,
            DeviceId = 123,
            DeviceType = LegacyPaymentType.PayPal
        };

        var userId = "aru-25198";
        var isResellerCustomer = false;
        var projectId = "project-xyz";

        var mapped = providerResource.MapToLegacyResource(userId, isResellerCustomer, projectId);

        mapped.Should().NotBeNull();
        mapped.Id.Should().Be(providerResource.Id);
        mapped.Name.Should().Be(providerResource.Name);
        mapped.MonthlyUnitPrice.Should().Be(providerResource.Price);
        mapped.DueDate.Should().Be(providerResource.ExpiringDate);
        mapped.AutoRenewMonths.Should().Be(providerResource.AutoRenewFrequency);
        mapped.Status.Should().Be(providerResource.Status);
        mapped.TypologyId.Should().Be("server");
        mapped.ShowVat.Should().BeTrue();
        mapped.AutoRenewEnabled.Should().Be(providerResource.AutoRenewEnabled);
        mapped.AutoRenewDeviceId.Should().NotBeNull();
        mapped.Uri.Should().NotBeNullOrEmpty();
        mapped.UserId.Should().Be(userId);
    }

    [Fact]
    [Unit]
    public void LegacyResourceProvider_List_MapToResource_Success()
    {
        var providerResources = new List<Abstractions.Providers.Models.Legacy.Internal.LegacyResource>()
        {
            new Abstractions.Providers.Models.Legacy.Internal.LegacyResource()
            {
                Id = 1,
                Name = "Resource 1",
                Price = 10.0m,
                ExpiringDate = DateTime.Now.AddDays(10),
                AutoRenewFrequency = 1,
                Status = "Active",
                ServiceType =  LegacyServiceType.Server,
                AutoRenewEnabled = true,
                DeviceId = 111,
                DeviceType = LegacyPaymentType.Sdd
            },
            new Abstractions.Providers.Models.Legacy.Internal.LegacyResource()
            {
                Id = 2,
                Name = "Resource 2",
                Price = 20.0m,
                ExpiringDate = DateTime.Now.AddDays(20),
                AutoRenewFrequency = 2,
                Status = "Active",
                ServiceType =  LegacyServiceType.Firewall,
                AutoRenewEnabled = false,
                DeviceId = 222,
                DeviceType = LegacyPaymentType.PayPal
            }
        };

        var userId = "user-xyz";
        var isResellerCustomer = false;
        var projectId = "project-abc";

        var mappedList = providerResources.MapToResource(userId, isResellerCustomer, projectId).ToList();

        mappedList.Should().NotBeNull();
        mappedList.Should().HaveCount(2);
        mappedList[0].Id.Should().Be(1);
        mappedList[1].Id.Should().Be(2);
    }

    [Fact]
    [Unit]
    public void LegacyResourceProvider_List_MapToBaseLegacyResource_Success()
    {
        var providerResources = new List<Abstractions.Providers.Models.Legacy.Internal.LegacyResource>()
    {
        new Abstractions.Providers.Models.Legacy.Internal.LegacyResource()
        {
            Id = 10,
            Name = "Resource-1",
            Price = 15.0m,
            ExpiringDate = DateTime.Now.AddDays(7),
            AutoRenewFrequency = 1,
            Status = "Active",
            ServiceType = LegacyServiceType.Server,
            AutoRenewEnabled = true,
            DeviceId = 101,
            DeviceType = LegacyPaymentType.PayPal
        },
        new Abstractions.Providers.Models.Legacy.Internal.LegacyResource()
        {
            Id = 20,
            Name = "Resource-2",
            Price = 30.0m,
            ExpiringDate = DateTime.Now.AddDays(14),
            AutoRenewFrequency = 2,
            Status = "Inactive",
            ServiceType = LegacyServiceType.Server,
            AutoRenewEnabled = false,
            DeviceId = 202,
            DeviceType = LegacyPaymentType.Sdd
        }
    };

        var userId = "user-test";

        var mappedList = providerResources.MapToBaseLegacyResource(userId).ToList();

        mappedList.Should().NotBeNull();
        mappedList.Should().HaveCount(2);

        mappedList[0].Id.Should().Be(10);
        mappedList[0].Name.Should().Be("Resource-1");
        mappedList[0].MonthlyUnitPrice.Should().Be(15.0m);
        mappedList[0].DueDate.Should().Be(providerResources[0].ExpiringDate);
        mappedList[0].AutoRenewMonths.Should().Be(1);
        mappedList[0].Status.Should().Be("Active");
        mappedList[0].TypologyId.Should().Be("server");
        mappedList[0].AutoRenewEnabled.Should().BeTrue();
        mappedList[0].AutoRenewDeviceId.Should().NotBeNull();
        mappedList[0].UserId.Should().Be(userId);

        mappedList[1].Id.Should().Be(20);
        mappedList[1].Name.Should().Be("Resource-2");
        mappedList[1].MonthlyUnitPrice.Should().Be(30.0m);
        mappedList[1].DueDate.Should().Be(providerResources[1].ExpiringDate);
        mappedList[1].AutoRenewMonths.Should().Be(2);
        mappedList[1].Status.Should().Be("Inactive");
        mappedList[1].TypologyId.Should().Be("server");
        mappedList[1].AutoRenewEnabled.Should().BeFalse();
        mappedList[1].AutoRenewDeviceId.Should().NotBeNull();
        mappedList[1].UserId.Should().Be(userId);
    }


    [Fact]
    [Unit]
    public void LegacyRegion_Map_Success()
    {
        var legacyRegions = new List<LegacyRegion>()
        {
            new LegacyRegion() { RegionId = "IT1", RegionName = "Bergamo" },
            new LegacyRegion() { RegionId = "IT2", RegionName = "Arezzo" }
        };

        var mapped = legacyRegions.Map().ToList();

        mapped.Should().NotBeNull();
        mapped.Should().HaveCount(2);
        mapped[0].Id.Should().Be("IT1");
        mapped[0].Code.Should().Be("Bergamo");
        mapped[1].Id.Should().Be("IT2");
        mapped[1].Code.Should().Be("Arezzo");
    }

    #endregion
}
