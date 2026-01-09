using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class ProductCatalogDtoBuilder
{
    public static ProductCatalogDtoBuilder New() => new ProductCatalogDtoBuilder();

    public ProductCatalogDto Instance { get; private init; }

    public ProductCatalogDtoBuilder()
    {
        Instance = new ProductCatalogDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Products = new List<ProductDto>() { ProductDtoBuilder.New().Instance };
    }
}
