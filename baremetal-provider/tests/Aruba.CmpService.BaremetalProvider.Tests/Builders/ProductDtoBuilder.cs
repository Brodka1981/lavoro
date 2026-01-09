using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class ProductDtoBuilder
{
    public static ProductDtoBuilder New() => new ProductDtoBuilder();

    public ProductDto Instance { get; private init; }

    public ProductDtoBuilder()
    {
        Instance = new ProductDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.VariantId = Guid.NewGuid().ToString();
        Instance.VariantName = Guid.NewGuid().ToString();
        Instance.ExtraInfo = new ProductExtraInfoDto()
        {
            ProvisioningProductId = Guid.NewGuid().ToString(),
            ProvisioningProductName = Guid.NewGuid().ToString(),
        };
        Instance.Plans = new List<PricingPlanDto>()
        {
            new PricingPlanDto()
            {
                ProductPlanPhases = new List<ProductPlanPhasesDto>()
                {
                    new ProductPlanPhasesDto()
                    {
                        BillingPeriod = "Hour"
                    }
                }
            }
        };
    }
}
