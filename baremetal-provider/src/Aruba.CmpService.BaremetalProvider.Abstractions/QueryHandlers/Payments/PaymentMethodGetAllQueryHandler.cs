using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments;
public class PaymentMethodGetAllQueryHandler :
    IQueryHandler<PaymentMethodsAllRequest, IEnumerable<PaymentMethod>>
{
    private readonly IPaymentsService paymentsService;

    public PaymentMethodGetAllQueryHandler(IPaymentsService paymentsService)
    {
        this.paymentsService = paymentsService;
    }
    public async Task<IEnumerable<PaymentMethod>> Handle(PaymentMethodsAllRequest request)
    {
        var result = await paymentsService.GetPaymentMethodsAsync(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }
}
