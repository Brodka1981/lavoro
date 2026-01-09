using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class EcommerceDtoBuilder
{
    public static EcommerceDtoBuilder New() => new EcommerceDtoBuilder();

    public EcommerceDto Instance { get; private init; }

    public EcommerceDtoBuilder()
    {
        Instance = new EcommerceDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.SubscriptionServices = SubscriptionServicesDtoBuilder.New().Instance;
        Instance.ProductCatalog = ProductCatalogDtoBuilder.New().Instance;
    }

    public EcommerceDtoBuilder WithSubscriptionServices(SubscriptionServicesDto? value)
    {
        Instance.SubscriptionServices = value;
        return this;
    }
}
