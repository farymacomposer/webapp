using System.Net.Http.Json;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Requests;
using Faryma.Composer.Desktop.Services.ComposerStreamFeature.Responses;
using Faryma.Composer.Desktop.Shared.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature
{
    public sealed class ComposerStreamService(IHttpClientFactory httpClientFactory, ILogger<ComposerStreamService> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient("Faryma.Composer.Api");

        public async Task<IEnumerable<ComposerStreamDto>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            QueryString queryBuilder = QueryString.Empty
                .Add("DateFrom", dateFrom.ToString("yyyy-MM-dd"))
                .Add("DateTo", dateTo.ToString("yyyy-MM-dd"));

            string url = $"/api/ComposerStream/FindStreams{queryBuilder}";

            StreamsResponse response = (await HttpClient.GetFromJsonAsync<StreamsResponse>(url))!;

            logger.LogInformation("{@response}", response);

            return response.Streams;
        }

        public async Task<IEnumerable<ComposerStreamDto>> FindLiveAndPlanned()
        {
            StreamsResponse response = (await HttpClient.GetFromJsonAsync<StreamsResponse>("/api/ComposerStream/FindLiveAndPlanned"))!;

            logger.LogInformation("{@response}", response);

            return response.Streams;
        }

        public async Task<ComposerStreamDto> Post<T>(T request)
        {
            string requestUri = request switch
            {
                CreateStreamRequest => "/api/ComposerStream/CreateStream",
                StartStreamRequest => "/api/ComposerStream/StartStream",
                CompleteStreamRequest => "/api/ComposerStream/CompleteStream",
                CancelStreamRequest => "/api/ComposerStream/CancelStream",
                _ => throw new InvalidOperationException()
            };

            HttpResponseMessage responseMessage = await HttpClient.PostAsJsonAsync(requestUri, request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();

                StreamResponse response = await responseMessage.Content.ReadFromJsonAsync<StreamResponse>()
                    ?? throw new InvalidOperationException();

                return response.ComposerStream;
            }
            catch (Exception ex)
            {
                string content = await responseMessage.Content.ReadAsStringAsync();
                logger.LogError("{content}", content);

                throw new InvalidOperationException(content, ex);
            }
        }
    }
}