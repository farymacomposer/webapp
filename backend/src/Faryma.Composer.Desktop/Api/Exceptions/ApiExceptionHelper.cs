using System.Net.Http.Json;
using Faryma.Composer.Contracts.Exceptions;

namespace Faryma.Composer.Desktop.Api.Exceptions
{
    public static class ApiExceptionHelper
    {
        public static async Task EnsureSuccessStatusCode(HttpResponseMessage responseMessage)
        {
            try
            {
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when ((int?)ex.StatusCode == AppException.StatusCode)
            {
                ResultObject? result = await responseMessage.Content.ReadFromJsonAsync<ResultObject>();

                if (result is null)
                {
                    string message = await responseMessage.Content.ReadAsStringAsync();

                    throw new InvalidOperationException(message, ex);
                }

                throw new ApiException(result, ex);
            }
            catch (Exception ex)
            {
                string message = await responseMessage.Content.ReadAsStringAsync();

                throw new InvalidOperationException(message, ex);
            }
        }
    }
}