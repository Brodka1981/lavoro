namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
public class FailureError :
    IServiceResultError
{

    public string? ErrorMessage { get; set; }
}