using Aruba.CmpService.ResourceProvider.Common;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Headers;

public class FromCeSourceAttribute :
    FromHeaderAttribute
{
    public FromCeSourceAttribute()
    {
        this.Name = RequestHeaders.Source;
    }
}
