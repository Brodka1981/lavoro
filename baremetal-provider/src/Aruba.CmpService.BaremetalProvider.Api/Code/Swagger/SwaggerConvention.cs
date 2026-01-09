using System.Collections;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;

public class SwaggerConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.SelectMany(sm => sm.Actions).ToList())
        {
            var httpMethodAttribute = action.Attributes.OfType<HttpMethodAttribute>().FirstOrDefault().HttpMethods.First();
            if (!SwaggerConstants.VoidActionResults.Contains(action.ActionMethod.ReturnType))
            {
                var returnType = GetMethodType(action.ActionMethod.ReturnType);

                switch (httpMethodAttribute)
                {
                    case "GET":
                        action.AddProducesResponse(HttpStatusCode.OK, returnType);

                        if (returnType == typeof(string) || returnType.GetInterface(nameof(IEnumerable)) == null)
                            action.AddProducesResponse(HttpStatusCode.NotFound);

                        break;
                    case "POST":
                        if (returnType == typeof(Task))
                        {
                            action.AddProducesResponse(HttpStatusCode.NoContent);
                        }
                        else
                        {
                            var producePostAttribute = action.Attributes.OfType<Produce200FamilyAttribute>().FirstOrDefault();
                            if (producePostAttribute != null)
                            {
                                action.AddProducesResponse((HttpStatusCode)producePostAttribute.StatusCode, returnType);
                            }
                            else
                            {
                                action.AddProducesResponse(HttpStatusCode.Created, returnType);
                            }
                        }

                        action.AddProducesResponse(HttpStatusCode.BadRequest, typeof(ProblemDetails));

                        break;
                    case "PUT":
                        var producePutAttribute = action.Attributes.OfType<Produce200FamilyAttribute>().FirstOrDefault();
                        if (producePutAttribute != null)
                        {
                            action.AddProducesResponse((HttpStatusCode)producePutAttribute.StatusCode, returnType);
                        }
                        else
                        {
                            action.AddProducesResponse(HttpStatusCode.Accepted, returnType);
                        }
                        action.AddProducesResponse(HttpStatusCode.BadRequest, typeof(ProblemDetails));
                        action.AddProducesResponse(HttpStatusCode.NotFound);

                        break;
                    case "PATCH":
                        action.AddProducesResponse(HttpStatusCode.OK, returnType);
                        action.AddProducesResponse(HttpStatusCode.BadRequest, typeof(ProblemDetails));
                        action.AddProducesResponse(HttpStatusCode.NotFound);

                        break;
                    case "DELETE":
                        var produceDeleteAttribute = action.Attributes.OfType<Produce200FamilyAttribute>().FirstOrDefault();
                        if (produceDeleteAttribute != null)
                        {
                            action.AddProducesResponse((HttpStatusCode)produceDeleteAttribute.StatusCode, returnType);
                        }
                        else
                        {
                            action.AddProducesResponse(HttpStatusCode.Accepted, returnType);
                        }
                        action.AddProducesResponse(HttpStatusCode.Accepted, returnType);
                        action.AddProducesResponse(HttpStatusCode.NotFound);

                        break;
                    default:
                        throw new NotImplementedException("Wrong HttpMethod");
                }
            }
            else
            {
                action.AddProducesResponse(HttpStatusCode.NoContent);
                action.AddProducesResponse(HttpStatusCode.BadRequest, typeof(ProblemDetails));
                switch (httpMethodAttribute)
                {
                    case "GET":
                        break;
                    case "POST":
                        break;
                    case "PUT":
                        break;
                    case "PATCH":
                        break;
                    case "DELETE":
                        action.AddProducesResponse(HttpStatusCode.NotFound);
                        break;
                    default:
                        throw new NotImplementedException("Wrong HttpMethod");
                }
            }
            action.AddProducesResponse(HttpStatusCode.InternalServerError);

            if (!action.Attributes.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)) &&
                (action.Attributes.Any(a => typeof(AuthorizeAttribute).IsAssignableFrom(a.GetType())) ||
                    action.Controller.Attributes.Any(a => typeof(AuthorizeAttribute).IsAssignableFrom(a.GetType()))))
            {
                action.AddProducesResponse(HttpStatusCode.Unauthorized);
                action.AddProducesResponse(HttpStatusCode.Forbidden);
            }
        }
    }

    static Type GetMethodType(Type returnType)
    {
        while (returnType.IsGenericType &&
                (returnType.GetGenericTypeDefinition() == typeof(ActionResult<>) ||
                    returnType.GetGenericTypeDefinition() == typeof(Task<>)))
            returnType = returnType.GenericTypeArguments.First();

        return returnType;
    }
}
