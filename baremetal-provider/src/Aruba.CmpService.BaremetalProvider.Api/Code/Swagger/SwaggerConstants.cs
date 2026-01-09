using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;

public class SwaggerConstants
{
    public static Type[] VoidActionResults = new Type[6] { typeof(ActionResult), typeof(IActionResult), typeof(Task<ActionResult>), typeof(Task<IActionResult>), typeof(Task), typeof(void) };
}
