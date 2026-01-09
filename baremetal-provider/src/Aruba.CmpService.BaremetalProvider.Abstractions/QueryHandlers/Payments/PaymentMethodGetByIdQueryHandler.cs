using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments;
public class PaymentMethodGetByIdQueryHandler :
    IQueryHandler<PaymentMethodByIdRequest, PaymentMethod?>
{
    private readonly IPaymentsService paymentsService;

    public PaymentMethodGetByIdQueryHandler(IPaymentsService paymentsService)
    {
        this.paymentsService = paymentsService;
    }
    public async Task<PaymentMethod?> Handle(PaymentMethodByIdRequest request)
    {
        var result = await paymentsService.GetPaymentMethodByIdAsync(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }
}
