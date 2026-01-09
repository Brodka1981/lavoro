using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using FluentAssertions;
using Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers;
public class QueryServiceTest : TestBase
{
    public QueryServiceTest(ITestOutputHelper output) : base(output)
    { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IQueryService, QueryService>();
        services.AddSingleton<IQueryHandler<SumQueryHandlerRequest, SumQueryHandlerResponse>, SumQueryHandler>();
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SumOk()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var qService = provider.GetService<IQueryService>();
        var response = await qService.ExecuteHandler<SumQueryHandlerRequest, SumQueryHandlerResponse>(new()
        {
            Field1 = 1,
            Field2 = 2,
        });
        response.Sum.Should().Be(3);
    }

    [Fact]
    [Unit]
    public async Task HandleAsync_SumNoOk()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var qService = provider.GetService<IQueryService>();
        var response = await qService.ExecuteHandler<SumQueryHandlerRequest, SumQueryHandlerResponse>(new()
        {
            Field1 = 2,
            Field2 = 3,
        });
        response.Sum.Should().NotBe(3);
    }
}
internal class SumQueryHandler :
    IQueryHandler<SumQueryHandlerRequest, SumQueryHandlerResponse>
{
    public async Task<SumQueryHandlerResponse?> Handle(SumQueryHandlerRequest request)
    {
        var ret = new SumQueryHandlerResponse()
        {
            Sum = request.Field1 + request.Field2,
        };

        return await Task.FromResult(ret);
    }
}

internal class SumQueryHandlerRequest
{
    public int Field1 { get; set; }
    public int Field2 { get; set; }
}

internal class SumQueryHandlerResponse
{
    public int Sum { get; set; }
}
