using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace Aruba.CmpService.BaremetalProvider.Api.Code.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerHeaderAttribute : Attribute
{
    public const string APIDOC = "APIDOC";
    public string HeaderName { get; }
    public string Description { get; }
    public string DefaultValue { get; }
    public bool IsRequired { get; }

    public SwaggerHeaderAttribute(string headerName, string description = null, string defaultValue = null, bool isRequired = false)
    {
        HeaderName = headerName;
        Description = description;
        DefaultValue = defaultValue;
        IsRequired = isRequired;
    }
}

public class SwaggerHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();


        if (context.MethodInfo.GetCustomAttributes(typeof(SwaggerHeaderAttribute)) is IList<SwaggerHeaderAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                var existingParam = operation.Parameters.FirstOrDefault(p =>
                         p.In == ParameterLocation.Header && p.Name == attribute.HeaderName);
                if (existingParam != null) // remove description from [FromHeader] argument attribute
                {
                    operation.Parameters.Remove(existingParam);
                }
                if (attribute.HeaderName == SwaggerHeaderAttribute.APIDOC)
                {
                    operation.Extensions = new Dictionary<string, IOpenApiExtension>() { { SwaggerHeaderAttribute.APIDOC, new OpenApiBoolean(true) } };

                }
                else
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = attribute.HeaderName,
                        In = ParameterLocation.Header,
                        Description = attribute.Description,
                        Required = attribute.IsRequired,
                        Schema = string.IsNullOrEmpty(attribute.DefaultValue)
                            ? null
                            : new OpenApiSchema
                            {
                                Type = "String",
                                Default = new OpenApiString(attribute.DefaultValue)
                            }
                    });

                }
            }

        }
    }
}
