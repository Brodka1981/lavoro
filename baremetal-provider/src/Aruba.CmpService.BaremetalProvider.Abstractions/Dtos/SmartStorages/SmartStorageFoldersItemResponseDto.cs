using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageFoldersItemResponseDto
{
    public string? Name { get; set; }
    public string? SmartFolderID { get; set; }
    public string? PositionDisplay { get; set; }
    public string? UsedSpace { get; set; }
    public string? AvailableSpace { get; set; }
    public bool Readonly { get; set; }
    public bool IsRootFolder { get; set; }

}
