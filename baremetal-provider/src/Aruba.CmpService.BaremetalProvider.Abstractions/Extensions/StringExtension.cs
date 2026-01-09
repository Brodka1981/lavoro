using System.Text;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
public static class StringExtensions
{
    public static string? ToBase64(this string value)
    {
        if(value is null)
        {
            return null;
        }
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
    }
}
