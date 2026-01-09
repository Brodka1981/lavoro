using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Extensions;

public static class ResponseExtensions
{
    public static ResponseDto<T> CleanEcommerce<T>(this ResponseDto<T> response, string? ceSource) where T : PropertiesBaseResponseDto
    {
        if (string.IsNullOrWhiteSpace(ceSource))
        {
            response.Metadata.Ecommerce = null;
        }
        return response;
    }
    public static ListResponseDto<T> CleanEcommerce<T, C>(this ListResponseDto<T> response, string? ceSource)
        where T : ResponseDto<C>
        where C : PropertiesBaseResponseDto
    {
        if (string.IsNullOrWhiteSpace(ceSource))
        {
            foreach (var item in response.Values)
            {
                item.CleanEcommerce(ceSource);
            }
        }

        return response;
    }
}
