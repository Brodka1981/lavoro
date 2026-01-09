using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageMetadataResponseDto
{
    public Dictionary<string, string> SmartStorageAiGuides { get; init; } = new();

}
