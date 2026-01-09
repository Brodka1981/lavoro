using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Jobs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Api.Code;
using Aruba.CmpService.BaremetalProvider.Api.Code.Extensions;
using Aruba.CmpService.BaremetalProvider.Api.Code.Security;
using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
using Aruba.CmpService.BaremetalProvider.Dependencies;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.AspNetCore;
using Aruba.CmpService.SecurityProvider.Api.Observability;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityServices<AppObservabilityServicesConfigurator>();

builder.Services.Configure<BaremetalOptions>(builder.Configuration.GetSection("Baremetal"));
builder.Services.Configure<RenewFrequencyOptions>(builder.Configuration.GetRequiredSection("RenewFrequency"));
builder.Services.Configure<EnableUpdatedEventOptions>(builder.Configuration.GetRequiredSection("EnableUpdatedEvent"));
builder.Services.Configure<LegacyAdminCredentials>(builder.Configuration.GetRequiredSection("LegacyAdminCredentials"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenVoidValidationParameters();
                    options.SaveToken = true;
                });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddArubaProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.ConfigureOptions());


builder.Services.AddControllers(options =>
                {
                    options.Conventions.Add(new SwaggerConvention());
                    options.AddResourceQuery();
                })
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.ConfigureOptions();
                });
builder.Services.AddRouting();

builder.Services.AddHealthChecks();

builder.Services.AddSwagger(builder.Environment);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddBaremetalProviderServices(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddHangFire(builder.Configuration);

builder.Services.AddHeaderPropagation(o =>
{
    o.Headers.Add("Authorization");
});

var app = builder.Build();

// Avvia il servizio per pianificare i job
using (var scope = app.Services.CreateScope())
{
    var jobScheduler = scope.ServiceProvider.GetRequiredService<IDeleteTokensJob>();
    jobScheduler.Execute();
}
app.UseArubaProblemDetails();

app.UseRouting();

app.UseHeaderPropagation();

var canExposeSensitiveInfo = builder.Configuration.GetValue<bool>("EnableSwagger");
if (canExposeSensitiveInfo)
{
    var apiVersionDescProvider = app.Services
        .GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger(options =>
    {
        options.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            var scheme = httpReq.Headers["X-Forwarded-Proto"].Count > 0 ? httpReq.Headers["X-Forwarded-Proto"].First() : httpReq.Scheme;
            var host = httpReq.Headers["X-Forwarded-Host"].Count > 0 ? httpReq.Headers["X-Forwarded-Host"].First() : httpReq.Host.Value;
            var swaggerUrl = $"{scheme}://{host}/{httpReq.Headers["X-Forwarded-Prefix"]}";

            swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = swaggerUrl } };
        });
    });

    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Swagger - Baremetal provider";
    });
}

app.UseAuthentication()
   .UseAuthorization();

app.MapObservabilityRoutes()
   .AllowAnonymous();

app.UseGatewayForwardedHeaderMiddleware();

app.MapControllers()
    .AddApplicationPortFilter();

var canExposeApiMetadata = !app.Environment.IsProduction();
if (canExposeApiMetadata)
{
    _ = app.MapOpenApiEndpoints()
           .AllowAnonymous()
           .AddApplicationPortFilter();
}

app.Run();