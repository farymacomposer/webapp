using System.Net.Http.Json;
using System.Text.Json;
using Faryma.Composer.Desktop.Api.ComposerStream.Responses;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Api.Shared.Dto;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.Http;

namespace Faryma.Composer.Desktop.Api.ComposerStream
{
    public sealed class ComposerStreamHttpClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        public async Task<IEnumerable<ComposerStreamDto>> Find(DateOnly dateFrom, DateOnly dateTo)
        {
            QueryString queryBuilder = QueryString.Empty
                .Add("DateFrom", dateFrom.ToString("yyyy-MM-dd"))
                .Add("DateTo", dateTo.ToString("yyyy-MM-dd"));

            string url = $"/api/ComposerStream/FindStreams{queryBuilder}";

            StreamsResponse response = await httpClient.GetFromJsonAsync<StreamsResponse>(url, serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать StreamResponse");

            return response.Streams;
        }

        public async Task<IEnumerable<ComposerStreamDto>> FindLiveAndPlanned()
        {
            StreamsResponse response = await httpClient.GetFromJsonAsync<StreamsResponse>("/api/ComposerStream/FindLiveAndPlanned", serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать StreamResponse");

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
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync(requestUri, request, serializerOptions);

            await ApiExceptionHelper.EnsureSuccessStatusCode(responseMessage);

            StreamResponse response = await responseMessage.Content.ReadFromJsonAsync<StreamResponse>(serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать StreamResponse");

            return response.ComposerStream;
        }
    }
}