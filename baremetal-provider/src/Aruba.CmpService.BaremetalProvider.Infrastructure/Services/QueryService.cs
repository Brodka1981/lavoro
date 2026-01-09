using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;

public class QueryService : IQueryService
{
    private readonly IServiceProvider serviceProvider;

    public QueryService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<TResponse?> ExecuteHandler<TRequest, TResponse>(TRequest request)
    {
        await using var serviceProviderScope = serviceProvider.CreateAsyncScope();

        var scopedServiceProvider = serviceProviderScope.ServiceProvider;
        var handler = scopedServiceProvider.GetService<IQueryHandler<TRequest, TResponse>>();

        return await handler.Handle(request).ConfigureAwait(false);
    }
}
