using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Desktop.Api.ComposerStream.Responses;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.Http;

namespace Faryma.Composer.Desktop.Api.ComposerStream
{
    public sealed class ComposerStreamHttpClient(HttpClient httpClient)
    {
        private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<IEnumerable<ComposerStreamDto>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            QueryString queryBuilder = QueryString.Empty
                .Add("DateFrom", dateFrom.ToString("yyyy-MM-dd"))
                .Add("DateTo", dateTo.ToString("yyyy-MM-dd"));

            string url = $"/api/ComposerStream/FindStreams{queryBuilder}";

            StreamsResponse response = (await httpClient.GetFromJsonAsync<StreamsResponse>(url, _serializerOptions))!;

            return response.Streams;
        }

        public async Task<IEnumerable<ComposerStreamDto>> FindLiveAndPlanned()
        {
            StreamsResponse response = (await httpClient.GetFromJsonAsync<StreamsResponse>("/api/ComposerStream/FindLiveAndPlanned", _serializerOptions))!;

            return response.Streams;
        }

        public Task<ComposerStreamDto> Create(DateOnly eventDate, ComposerStreamType type) => Post("/api/ComposerStream/CreateStream", new
        {
            EventDate = eventDate,
            Type = type,
        });

        public Task<ComposerStreamDto> Start(long composerStreamId) => Post("/api/ComposerStream/StartStream", new
        {
            ComposerStreamId = composerStreamId,
        });

        public Task<ComposerStreamDto> Complete(long composerStreamId) => Post("/api/ComposerStream/CompleteStream", new
        {
            ComposerStreamId = composerStreamId,
        });

        public Task<ComposerStreamDto> Cancel(long composerStreamId) => Post("/api/ComposerStream/CancelStream", new
        {
            ComposerStreamId = composerStreamId,
        });

        private async Task<ComposerStreamDto> Post<T>(string requestUri, T request)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync(requestUri, request, _serializerOptions);

            try
            {
                responseMessage.EnsureSuccessStatusCode();

                StreamResponse response = await responseMessage.Content.ReadFromJsonAsync<StreamResponse>()
                    ?? throw new InvalidOperationException();

                return response.ComposerStream;
            }
            catch (HttpRequestException ex) when ((int?)ex.StatusCode == 600)
            {
                ResultObject result = await responseMessage.Content.ReadFromJsonAsync<ResultObject>()
                    ?? throw new InvalidOperationException();

                throw new ApiException(result);
            }
        }
    }
}