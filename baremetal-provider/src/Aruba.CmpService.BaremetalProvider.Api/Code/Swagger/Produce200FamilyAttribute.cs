using System.Net;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public sealed class Produce200FamilyAttribute : Attribute
{
    public Produce200FamilyAttribute(int statusCode)
    {
        StatusCode = statusCode;
        if (statusCode < 200 || statusCode > 299)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Invalid status code");
        }
    }
    public Produce200FamilyAttribute(HttpStatusCode statusCode) :
        this((int)statusCode)
    {

    }

    public int StatusCode { get; }
}
