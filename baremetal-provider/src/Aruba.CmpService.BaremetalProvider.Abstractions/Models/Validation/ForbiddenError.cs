namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
public class ForbiddenError :
    IServiceResultError
{
    public ForbiddenError(object? id = null)
    {
        this.Id = id;
    }

    public object? Id { get; set; }
}