using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class IdentityDtoBuilder
{
    public static IdentityDtoBuilder New() => new IdentityDtoBuilder();

    public IdentityDto Instance { get; private init; }

    public IdentityDtoBuilder()
    {
        Instance = new IdentityDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.UserId = Guid.NewGuid().ToString();
       // Instance.Username = Guid.NewGuid().ToString();
        Instance.Account = Guid.NewGuid().ToString();
        Instance.Tenant = Guid.NewGuid().ToString();
        Instance.Company = Guid.NewGuid().ToString();
    }

    public IdentityDtoBuilder WithUserId(string? value)
    {
        Instance.UserId = value;
        return this;
    }
}
