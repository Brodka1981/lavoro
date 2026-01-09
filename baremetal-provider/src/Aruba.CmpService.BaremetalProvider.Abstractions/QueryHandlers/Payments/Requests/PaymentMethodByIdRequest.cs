namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
public class PaymentMethodByIdRequest
{
    public string? UserId { get; set; }
    public string? Id { get; set; }
}
