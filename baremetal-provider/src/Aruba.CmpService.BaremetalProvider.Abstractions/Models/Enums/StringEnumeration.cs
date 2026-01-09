namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
public abstract class StringEnumeration
{
    public string Value { get; private set; }
    protected StringEnumeration(string value)
    {
        Value = value;
    }
}
