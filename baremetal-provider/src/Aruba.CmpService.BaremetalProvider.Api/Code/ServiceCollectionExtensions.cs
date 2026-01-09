using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Api.Code.Filters;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.AspNetCore;
using Aruba.CmpService.ResourceProvider.Common.Swagger;
using Asp.Versioning;

namespace Aruba.CmpService.BaremetalProvider.Api.Code;

[ExcludeFromCodeCoverage(Justification = "It's a class for dependency injection with facade methods")]
internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwagger(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddOpenApiServices()
            .AddApiVersioningStrategy(strategy => strategy.UseQueryStringParameter(
                environment,
                desc => new OpenApiInfo
                {
                    Title = "Aruba.BaremetalProvider.Api",
                    Version = desc.ApiVersion.ToString(),
                    Description = "Aruba.BaremetalProvider.Api HTTP API",
                    Contact = new OpenApiContact
                    {
                        Name = "Aruba.BaremetalProvider.Api",
                        Url = new Uri("https://www.aruba.it")
                    }
                },
                "api-version",
                options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.ReportApiVersions = true;
                },
                options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.SubstituteApiVersionInUrl = true;
                    options.GroupNameFormat = "'v'VVV";
                }
        ));

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.OperationFilter<SwaggerHeaderFilter>();
            options.SchemaFilter<AddSwaggerRequiredSchemaFilter>();
            options.SchemaFilter<HidePropertySwaggerSchemaFilter>();

            options.AddResourceQuery();

            options.CustomSchemaIds(type =>
            {
                switch (type.Name)
                {
                    case "LocationDto":
                    case "ProjectDto":
                    case "LinkedResourceDto":
                        //return string.Join(".", type.FullName.Split(".").Reverse().Take(2).Reverse());
                        return string.Join(".",
                            System.Linq.Enumerable.Reverse(
                                System.Linq.Enumerable.Take(
                                    System.Linq.Enumerable.Reverse(
                                        type.FullName.Split('.')
                                    ),
                                    2
                                )
                            )
                        );
                    default:
                        return type.FullName;
                }
            });

            var xmlFiles = System.IO.Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile);
            }
        });

        return services;
    }

    public static void ConfigureOptions(this JsonSerializerOptions options)
    {
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.Converters.Add(new JsonStringEnumConverter());
    }
}
