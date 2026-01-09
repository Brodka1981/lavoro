using Aruba.CmpService.ResourceProvider.Common.Dtos.FullPayload;
using Aruba.CmpService.BaremetalProvider.Tests.Builders;

namespace Aruba.CmpService.BaremetalProvider.Tests.Builders;

internal class ProjectDtoBuilder
{
    public static ProjectDtoBuilder New() => new ProjectDtoBuilder();

    public ProjectDto Instance { get; private init; }

    public ProjectDtoBuilder()
    {
        Instance = new ProjectDto();
        FillWithMockData();
    }

    private void FillWithMockData()
    {
        Instance.Id = Guid.NewGuid().ToString();
        Instance.Name = Guid.NewGuid().ToString();
    }
}
