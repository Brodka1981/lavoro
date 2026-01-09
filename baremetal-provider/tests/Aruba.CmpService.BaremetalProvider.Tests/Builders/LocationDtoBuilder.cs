using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class LocationDtoBuilder
{
    public static LocationDtoBuilder New() => new LocationDtoBuilder();

    public LocationDto Instance { get; private init; }

    public LocationDtoBuilder()
    {
        Instance = new LocationDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Code = Guid.NewGuid().ToString();
        Instance.Name = Guid.NewGuid().ToString();
        Instance.City = Guid.NewGuid().ToString();
        Instance.Country = Guid.NewGuid().ToString();
        Instance.Value = Guid.NewGuid().ToString();
    }
}
