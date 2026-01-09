namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
public class NotFoundError :
    IServiceResultError
{
    public NotFoundError(object? id)
    {
        this.Id = id;
    }
    public object? Id { get; set; }
}