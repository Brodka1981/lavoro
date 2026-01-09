using Aruba.CmpService.ResourceProvider.Common;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;

public static class ControllerExtensions
{
    public static bool IsExternalSource(this ControllerBase controller, string? source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}
