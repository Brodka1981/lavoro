using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
public class BadRequestError :
    IServiceResultError
{
    public string? FieldName { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorLabel { get; private set; }
    public Dictionary<string, object> Params { get; private set; } = new Dictionary<string, object>();

    public static BadRequestError Create(FieldNames fieldName, string errorMessage)
    {
        fieldName.ThrowIfNull();
        var ret = new BadRequestError()
        {
            FieldName = fieldName.Value,
            ErrorMessage = errorMessage
        };
        return ret;
    }

    public BadRequestError AddLabel(BaremetalBaseLabelErrors label)
    {
        this.ErrorLabel = label.Value;
        return this;
    }

    public BadRequestError AddLabel(LegacyLabelErrors label)
    {
        this.ErrorLabel = label.Label;
        return this;
    }
    public BadRequestError AddParam(string key, object value)
    {
        this.Params.Add(key, value);
        return this;
    }

}