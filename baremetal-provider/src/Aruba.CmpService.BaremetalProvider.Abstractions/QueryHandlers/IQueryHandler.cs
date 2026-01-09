namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
public interface IQueryHandler<TRequest, TResponse>
{
    Task<TResponse?> Handle(TRequest request);
}
