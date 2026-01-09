using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class InternalMapping
{
    #region Response
    public static IEnumerable<LegacyResourceResponseDto> MapToResponse([NotNull] this IEnumerable<Models.Internal.LegacyResource> resources)
    {
        return resources.Select(s => s.MapToResponse()).ToList()!;
    }

    public static LegacyResourceResponseDto MapToResponse([NotNull] this Models.Internal.LegacyResource resource)
    {
        var ret = new LegacyResourceResponseDto()
        {
            BillingPeriod = resource.BillingPeriod,
            Id = resource.Id,
            MonthlyUnitPrice = resource.MonthlyUnitPrice,
            AutoRenewDeviceId = resource.AutoRenewDeviceId,
            AutoRenewEnabled = resource.AutoRenewEnabled,
            Name = resource.Name,
            Uri = resource.Uri,
            DueDate = resource.DueDate,
            AutoRenewMonths = resource.AutoRenewMonths,
            ShowVat = false,
            Status = resource.Status,
            TypologyId = resource.TypologyId
        };
        return ret;
    }
    public static IEnumerable<BaseLegacyResourceResponseDto> MapToResponse([NotNull] this IEnumerable<BaseLegacyResource> resources)
    {
        return resources.Select(s => s.MapToResponse()).ToList()!;
    }

    public static BaseLegacyResourceResponseDto MapToResponse([NotNull] this BaseLegacyResource resource)
    {
        var ret = new BaseLegacyResourceResponseDto()
        {
            BillingPeriod = resource.BillingPeriod,
            Id = resource.Id,
            MonthlyUnitPrice = resource.MonthlyUnitPrice,
            AutoRenewDeviceId = resource.AutoRenewDeviceId,
            AutoRenewEnabled = resource.AutoRenewEnabled,
            Name = resource.Name,
            DueDate = resource.DueDate,
            AutoRenewMonths = resource.AutoRenewMonths,
            Status = resource.Status,
            TypologyId = resource.TypologyId
        };
        return ret;
    }

    public static AutorechargeResponse MapToResponse([NotNull] this LegacyAutorechargeData legacyAutorecharge)
    {
        return new AutorechargeResponse()
        {
            Enabled = legacyAutorecharge.Enabled,
            DeviceId = legacyAutorecharge.DeviceId,
            DeviceType = legacyAutorecharge.DeviceType,
            CreditToAutoRecharge = legacyAutorecharge.CreditToAutoRecharge,
        };
    }
    #endregion

    #region ToModel
    public static IEnumerable<Models.Internal.LegacyResource> MapToResource([NotNull] this IEnumerable<Providers.Models.Legacy.Internal.LegacyResource> resources, string userId, bool isResellerCustomer, string progectId)
    {
        return resources.Select(s => s.MapToLegacyResource(userId, isResellerCustomer, progectId)).ToList()!;
    }

    public static Models.Internal.LegacyResource MapToLegacyResource([NotNull] this Providers.Models.Legacy.Internal.LegacyResource resource, string userId, bool isResellerCustomer, string projectId)
    {
        var typology = resource.ServiceType.MapToCmpTypology();
        var uri = typology.CreateUri(projectId!, resource.Id);
        var showVat = resource.AutoRenewEnabled == true
            && resource.DeviceType.HasValue && resource.DeviceType != Providers.Models.Legacy.Payments.LegacyPaymentType.Wallet
            && isResellerCustomer == false;
        var autoRenewDeviceId = resource.DeviceId.HasValue && resource.DeviceType.HasValue ? PaymentExtensions.CreateCmpDeviceId(resource.DeviceId.Value, resource.DeviceType.Value) : null;
        var ret = new Models.Internal.LegacyResource()
        {
            BillingPeriod = "Month",
            Id = resource.Id,
            MonthlyUnitPrice = resource.Price!.Value,
            Name = resource.Name,
            DueDate = resource.ExpiringDate,
            AutoRenewMonths = resource.AutoRenewFrequency > 0 ? resource.AutoRenewFrequency : null,
            Status = resource.Status,
            TypologyId = typology.Value,
            ShowVat = showVat,
            AutoRenewEnabled = resource.AutoRenewEnabled,
            AutoRenewDeviceId = autoRenewDeviceId,
            Uri = uri,
            UserId = userId,
        };
        return ret;
    }

    public static IEnumerable<BaseLegacyResource> MapToBaseLegacyResource([NotNull] this IEnumerable<Providers.Models.Legacy.Internal.LegacyResource> resources, string userId)
    {
        return resources.Select(s => s.MapBaseLegacyResource(userId)).ToList()!;
    }

    private static BaseLegacyResource MapBaseLegacyResource([NotNull] this Providers.Models.Legacy.Internal.LegacyResource resource, string userId)
    {
        var typology = resource.ServiceType.MapToCmpTypology();

        var autoRenewDeviceId = resource.DeviceId.HasValue && resource.DeviceType.HasValue ? PaymentExtensions.CreateCmpDeviceId(resource.DeviceId.Value, resource.DeviceType.Value) : null;

        var ret = new BaseLegacyResource()
        {
            BillingPeriod = "Month",
            Id = resource.Id,
            MonthlyUnitPrice = resource.Price!.Value,
            Name = resource.Name,
            DueDate = resource.ExpiringDate,
            AutoRenewMonths = resource.AutoRenewFrequency > 0 ? resource.AutoRenewFrequency : null,
            Status = resource.Status,
            TypologyId = typology.Value,
            AutoRenewEnabled = resource.AutoRenewEnabled,
            AutoRenewDeviceId = autoRenewDeviceId,
            UserId = userId,
        };
        return ret;
    }

    public static IEnumerable<Region> Map(this IEnumerable<LegacyRegion> legacyRegions)
    {
        return legacyRegions.Select(r => new Region()
        {
            Id = r.RegionId,
            Code = r.RegionName,
        }).ToList();
    }

    #endregion
}