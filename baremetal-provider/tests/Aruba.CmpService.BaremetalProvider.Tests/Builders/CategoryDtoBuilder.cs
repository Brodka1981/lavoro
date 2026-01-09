using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class CategoryDtoBuilder<TExtraInfo>
{
    public static CategoryDtoBuilder<TExtraInfo> New() => new CategoryDtoBuilder<TExtraInfo>();

    public CategoryDto<TExtraInfo> Instance { get; private init; }

    public CategoryDtoBuilder()
    {
        Instance = new CategoryDto<TExtraInfo>();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Name = Guid.NewGuid().ToString();
        Instance.Typology  = TypologyDtoBuilder<TExtraInfo>.New().Instance;
    }
}
