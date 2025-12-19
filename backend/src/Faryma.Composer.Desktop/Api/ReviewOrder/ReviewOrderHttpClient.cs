using System.Net.Http.Json;
using System.Text.Json;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.MoveUp;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Api.ReviewOrder.Responses;

namespace Faryma.Composer.Desktop.Api.ReviewOrder
{
    public sealed class ReviewOrderHttpClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        public Task<ReviewOrderDto> Create(Guid idempotencyKey, CreateReviewOrderRequest request) => Post("/api/ReviewOrder/CreateReviewOrder", idempotencyKey, request);
        public Task<ReviewOrderDto> MoveUp(Guid idempotencyKey, MoveUpReviewOrderRequest request) => Post("/api/ReviewOrder/MoveUpReviewOrder", idempotencyKey, request);

        public Task<ReviewOrderDto> AddTrackUrl(long reviewOrderId, string trackUrl) => Post("/api/ReviewOrder/AddTrackUrl", new
        {
            ReviewOrderId = reviewOrderId,
            TrackUrl = trackUrl,
        });

        public Task<ReviewOrderDto> TakeOrderInProgress(long reviewOrderId) => Post("/api/ReviewOrder/TakeOrderInProgress", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Complete(long reviewOrderId, int rating) => Post("/api/ReviewOrder/CompleteReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
            Rating = rating,
        });

        public Task<ReviewOrderDto> Freeze(long reviewOrderId) => Post("/api/ReviewOrder/FreezeReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Unfreeze(long reviewOrderId) => Post("/api/ReviewOrder/UnfreezeReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Cancel(long reviewOrderId) => Post("/api/ReviewOrder/CancelReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        private async Task<ReviewOrderDto> Post<T>(string requestUri, T request)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync(requestUri, request, serializerOptions);

            await ApiExceptionHelper.EnsureSuccessStatusCode(responseMessage);

            ReviewOrderResponse response = await responseMessage.Content.ReadFromJsonAsync<ReviewOrderResponse>(serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать ReviewOrderResponse");

            return response.ReviewOrder;
        }

        private async Task<ReviewOrderDto> Post<T>(string requestUri, Guid idempotencyKey, T request)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, requestUri);
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request, options: serializerOptions);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            await ApiExceptionHelper.EnsureSuccessStatusCode(responseMessage);

            ReviewOrderResponse response = await responseMessage.Content.ReadFromJsonAsync<ReviewOrderResponse>(serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать ReviewOrderResponse");

            return response.ReviewOrder;
        }
    }
}