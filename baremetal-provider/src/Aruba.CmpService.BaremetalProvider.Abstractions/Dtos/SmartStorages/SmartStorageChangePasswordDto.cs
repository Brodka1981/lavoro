using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageChangePasswordDto
{
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
