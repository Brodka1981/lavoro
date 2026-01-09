using System.Net;
using System.Text;
using System.Text.Json;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
//
// Summary:
//     Esempi di extension methods per gestire la serializzazione/deserializzazione
//     di input e output di chiamate HTTP. Gestione implicita anche degli errori. Sono
//     comunque funzionali alle API chiamate, non hanno valenza generica per qualunque
//     servizio. Inoltre per comodità vengono usate le stesse classi in Common come
//     DTO sia in fase inbound che outbound. Solitamente il DTO gestito dal client è
//     un Model diverso come classe ma identico come struttura di quello gestito dalla
//     API (anche perchè alla fine è solo un oggetto che viene serializzato/deserializzato,
//     quello che viaggia è il json).
public static class HttpClientExtensions
{
    #region Public methods
    //
    // Summary:
    //     Effettua una chiamata get
    //
    // Parameters:
    //   httpClient:
    //
    //   action:
    //
    // Type parameters:
    //   TOutput:
    public static async Task<ApiCallOutput<TOutput>> CallGetAsync<TOutput>(this HttpClient httpClient, string action)
    {
        ArgumentNullException.ThrowIfNull(httpClient, "httpClient");
        try
        {
            if (!httpClient.BaseAddress.ToString().EndsWith("/"))
            {
                httpClient.BaseAddress = new Uri(httpClient.BaseAddress.ToString().TrimEnd('/') + "/");
            }
            if (!string.IsNullOrWhiteSpace(action))
            {
                action = action.TrimStart('/');
            }
            var message = await httpClient.GetAsync(action).ConfigureAwait(continueOnCapturedContext: false);
            if (message.IsSuccessStatusCode)
            {
                var output2 = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                return new ApiCallOutput<TOutput>
                {
                    Success = true,
                    StatusCode = message.StatusCode,
                    Result = output2.Deserialize<TOutput>()
                };
            }

            if (message.StatusCode == HttpStatusCode.NotFound)
            {
                return new ApiCallOutput<TOutput>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var output = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var apiCallOutput = new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = message.StatusCode
            };
            if (message.StatusCode == HttpStatusCode.InternalServerError)
            {
                apiCallOutput.InternalServerError = output;
            }
            else
            {
                apiCallOutput.Err = TryToDeserialize(output);
            }

            return apiCallOutput;
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                InternalServerError = ex.Message
            };
        }
    }

    public static async Task<ApiCallOutput> CallGetAsync(this HttpClient httpClient, string action)
    {
        ApiCallOutput<object> result = await httpClient.CallGetAsync<object>(action).ConfigureAwait(continueOnCapturedContext: false);
        return new ApiCallOutput
        {
            Success = result.Success,
            InternalServerError = result.InternalServerError,
            StatusCode = result.StatusCode,
            Err = result.Err
        };
    }

    //
    // Summary:
    //     Invocazione metodo POST su http client
    //
    // Parameters:
    //   httpClient:
    //     istanza httpclient
    //
    //   action:
    //     action da invocare
    //
    //   item:
    //     oggetto da passare
    //
    // Type parameters:
    //   TOutput:
    //     eventuale tipo in response
    public static async Task<ApiCallOutput<TOutput>> CallPostAsync<TOutput>(this HttpClient httpClient, string action, object item, ILogger logger = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient, "httpClient");
        StringContent httpContent = null;
        try
        {
            if (item != null)
            {
                var stringPayload = item.Serialize();
                httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            }

            if (!httpClient.BaseAddress.ToString().EndsWith("/"))
            {
                httpClient.BaseAddress = new Uri(httpClient.BaseAddress.ToString().TrimEnd('/') + "/");
            }
            if (!string.IsNullOrWhiteSpace(action))
            {
                action = action.TrimStart('/');
            }

            var message = await httpClient.PostAsync(action, httpContent).ConfigureAwait(continueOnCapturedContext: false);
            if (message.IsSuccessStatusCode)
            {
                var content = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                var result = TryToDeserialize<TOutput>(content);
                return new ApiCallOutput<TOutput>
                {
                    Success = true,
                    StatusCode = message.StatusCode,
                    Result = result
                };
            }

            var output = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            var apiCallOutput = new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = message.StatusCode
            };
            if (message.StatusCode == HttpStatusCode.InternalServerError)
            {
                apiCallOutput.InternalServerError = output;
            }
            else
            {
                apiCallOutput.Err = TryToDeserialize(output);
            }

            return apiCallOutput;
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                InternalServerError = ex.Message
            };
        }
        finally
        {
            if (httpContent != null)
            {
                httpContent.Dispose();
            }
        }
    }

    //
    // Summary:
    //     Effettua una chaimata post
    //
    // Parameters:
    //   httpClient:
    //
    //   action:
    //
    //   item:
    public static async Task<ApiCallOutput> CallPostAsync(this HttpClient httpClient, string action, object item)
    {
        ApiCallOutput<object> result = await httpClient.CallPostAsync<object>(action, item).ConfigureAwait(continueOnCapturedContext: false);
        return new ApiCallOutput
        {
            Success = result.Success,
            InternalServerError = result.InternalServerError,
            StatusCode = result.StatusCode,
            Err = result.Err
        };
    }

    //
    // Summary:
    //     Effettua una chaimata put
    //
    // Parameters:
    //   httpClient:
    //
    //   action:
    //
    //   item:
    public static async Task<ApiCallOutput> CallPutAsync(this HttpClient httpClient, string action, object item)
    {
        ApiCallOutput<object> result = await httpClient.CallPutAsync<object>(action, item).ConfigureAwait(continueOnCapturedContext: false);
        return new ApiCallOutput
        {
            Success = result.Success,
            InternalServerError = result.InternalServerError,
            StatusCode = result.StatusCode,
            Err = result.Err
        };
    }//
    // Summary:
    //     Invocazione metodo PUT su http client
    //
    // Parameters:
    //   httpClient:
    //     istanza httpclient
    //
    //   action:
    //     action da invocare
    //
    //   item:
    //     oggetto da passare
    //
    // Type parameters:
    //   TOutput:
    //     eventuale tipo in response
    public static async Task<ApiCallOutput<TOutput>> CallPutAsync<TOutput>(this HttpClient httpClient, string action, object item)
    {
        ArgumentNullException.ThrowIfNull(httpClient, "httpClient");
        StringContent httpContent = null;
        try
        {
            if (item != null)
            {
                var stringPayload = item.Serialize();
                httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            }

            var message = await httpClient.PutAsync(action, httpContent).ConfigureAwait(continueOnCapturedContext: false);
            if (message.IsSuccessStatusCode)
            {
                var result = TryToDeserialize<TOutput>(await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
                return new ApiCallOutput<TOutput>
                {
                    Success = true,
                    StatusCode = message.StatusCode,
                    Result = result
                };
            }

            var output = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var apiCallOutput = new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = message.StatusCode
            };
            if (message.StatusCode == HttpStatusCode.InternalServerError)
            {
                apiCallOutput.InternalServerError = output;
            }
            else
            {
                apiCallOutput.Err = TryToDeserialize(output);
            }

            return apiCallOutput;
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new ApiCallOutput<TOutput>
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                InternalServerError = ex.Message
            };
        }
        finally
        {
            if (httpContent != null)
            {
                httpContent.Dispose();
            }
        }
    }

    //
    // Summary:
    //     Effettua una chiamata delete
    //
    // Parameters:
    //   httpClient:
    //
    //   action:
    //
    //   item:
    public static async Task<ApiCallOutput> CallDeleteAsync(this HttpClient httpClient, string action, object item)
    {
        ArgumentNullException.ThrowIfNull(httpClient, "httpClient");
        HttpContent httpContent = null;
        try
        {
            if (item != null)
            {
                var stringPayload = item.Serialize();
                httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            }
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = string.IsNullOrEmpty(action) ? httpClient.BaseAddress : new Uri(new Uri($"{httpClient.BaseAddress}"), action),
                Content = httpContent
            };
            var message = await httpClient.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
            if (message.IsSuccessStatusCode)
            {
                return new ApiCallOutput
                {
                    Success = true,
                    StatusCode = message.StatusCode
                };
            }

            var output = await message.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var apiCallOutput = new ApiCallOutput
            {
                Success = false,
                StatusCode = message.StatusCode
            };
            if (message.StatusCode == HttpStatusCode.InternalServerError)
            {
                apiCallOutput.InternalServerError = output;
            }
            else
            {
                apiCallOutput.Err = TryToDeserialize(output);
            }

            return apiCallOutput;
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new ApiCallOutput
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                InternalServerError = ex.Message
            };
        }
        finally
        {
            if (httpContent != null)
            {
                httpContent.Dispose();
            }
        }
    }
    #endregion

    #region Private methods

    private static TOutput? TryToDeserialize<TOutput>(string output)
    {
        var result = default(TOutput);
        try
        {
            result = output.Deserialize<TOutput>();
            return result;
        }
        catch
        {
        }

        return result;
    }

    private static ApiError? TryToDeserialize(string output)
    {
        ApiError? apiError;
        try
        {
            apiError = output.Deserialize<ApiError>();
            if (apiError?.Code == null)
            {
                apiError = new ApiError
                {
                    Code = "GenericError",
                    Detail = output
                };
            }
        }
        catch
        {
            apiError = new ApiError
            {
                Code = "GenericError",
                Detail = output
            };
        }

        return apiError;
    }
    #endregion
}
