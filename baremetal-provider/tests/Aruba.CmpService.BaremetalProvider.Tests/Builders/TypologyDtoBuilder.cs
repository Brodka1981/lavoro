using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using MongoDB.Bson;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class TypologyDtoBuilder<TExtraInfo>
{
    public static TypologyDtoBuilder<TExtraInfo> New() => new TypologyDtoBuilder<TExtraInfo>();

    public TypologyDto<TExtraInfo> Instance { get; private init; }

    public TypologyDtoBuilder()
    {
        Instance = new TypologyDto<TExtraInfo>();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Id = ObjectId.GenerateNewId().ToString();
        Instance.Name = Guid.NewGuid().ToString();
        Instance.LimitResourceAvailableForLocation = 50;
    }
}
