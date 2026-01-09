namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
public interface IQueryService
{
    Task<TResponse?> ExecuteHandler<TRequest, TResponse>(TRequest request);
}
