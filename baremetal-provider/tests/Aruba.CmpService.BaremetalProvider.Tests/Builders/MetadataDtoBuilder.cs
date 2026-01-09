using System.Collections.ObjectModel;
using Aruba.CmpService.ResourceProvider.Common.Dtos.UserPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class MetadataDtoBuilder
{
    public static MetadataDtoBuilder New() => new MetadataDtoBuilder();

    public MetadataDto Instance { get; private init; }

    public MetadataDtoBuilder()
    {
        Instance = new MetadataDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Name = Guid.NewGuid().ToString();
        Instance.Location = new LocationDto()
        {
            Value = "Bergamo",
        };

        Instance.Tags = new Collection<string>();
    }
}
