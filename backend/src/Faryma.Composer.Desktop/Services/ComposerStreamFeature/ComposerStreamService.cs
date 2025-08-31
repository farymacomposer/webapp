using System.Net.Http.Json;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Cancel;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Complete;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Create;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Start;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature
{
    public sealed class ComposerStreamService(IHttpClientFactory httpClientFactory, ILogger<ComposerStreamService> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient("Faryma.Composer.Api");

        public async Task Initialize()
        {
            await Task.Delay(2000);
            await Task.Delay(2000);
        }

        public async Task Post<T>(T request)
        {
            string requestUri = request switch
            {
                CreateComposerStreamRequest => "/api/ComposerStream/CreateStream",
                StartStreamRequest => "/api/ComposerStream/StartStream",
                CompleteStreamRequest => "/api/ComposerStream/CompleteStream",
                CancelStreamRequest => "/api/ComposerStream/CancelStream",
                _ => throw new InvalidOperationException()
            };

            HttpResponseMessage responseMessage = await HttpClient.PostAsJsonAsync(requestUri, request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }
    }
}