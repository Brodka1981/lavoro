namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset ToDateTimeOffset(this DateTime? dateTime)
    {
        if (!dateTime.HasValue || dateTime.Value == DateTime.MinValue)
        {
            return DateTimeOffset.MinValue;
        }
        else
        {
            return new DateTimeOffset(dateTime.Value);
        }
    }
    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
    {
        return ((DateTime?)dateTime).ToDateTimeOffset();
    }
}
