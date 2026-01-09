using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class SubscriptionServicesDtoBuilder
{
    public static SubscriptionServicesDtoBuilder New() => new SubscriptionServicesDtoBuilder();

    public SubscriptionServicesDto Instance { get; private init; }

    public SubscriptionServicesDtoBuilder()
    {
        Instance = new SubscriptionServicesDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.OrderId = Guid.NewGuid().ToString();
        Instance.Services.Add(
            new ServiceDto()
            {
                OrderItemId = Guid.NewGuid().ToString(),
            });
    }

    public SubscriptionServicesDtoBuilder WithOrderId(string? value)
    {
        Instance.OrderId = value;
        return this;
    }
}
