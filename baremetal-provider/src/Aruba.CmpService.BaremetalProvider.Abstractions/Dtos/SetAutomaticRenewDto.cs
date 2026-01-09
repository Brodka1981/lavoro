using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SetAutomaticRenewDto
{
    public string? PaymentMethodId { get; set; }
    public byte? Months { get; set; }
    public bool Activate { get; set; }
    public AutorenewFolderAction? ActionOnFolder { get; set; }
}
