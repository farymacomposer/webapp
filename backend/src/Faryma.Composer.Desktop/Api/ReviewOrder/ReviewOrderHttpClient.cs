using System.Net.Http.Json;
using System.Text.Json;
using Faryma.Composer.Contracts.Api;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Pay;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Api.ReviewOrder.Responses;

namespace Faryma.Composer.Desktop.Api.ReviewOrder
{
    public sealed class ReviewOrderHttpClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        public Task<ReviewOrderDto> CreateDonation(Guid idempotencyKey, CreateDonationReviewOrderRequest request) => Post("/api/review-orders/create/donation", idempotencyKey, request);
        public Task<ReviewOrderDto> CreateOutOfQueue(Guid idempotencyKey, CreateOutOfQueueReviewOrderRequest request) => Post("/api/review-orders/create/out-of-queue", idempotencyKey, request);
        public Task<ReviewOrderDto> CreateFree(Guid idempotencyKey, CreateFreeReviewOrderRequest request) => Post("/api/review-orders/create/free", idempotencyKey, request);
        public Task<ReviewOrderDto> CreateToken(Guid idempotencyKey, CreateTokenReviewOrderRequest request) => Post("/api/review-orders/create/token", idempotencyKey, request);
        public Task<ReviewOrderDto> CreateCharity(Guid idempotencyKey, CreateCharityReviewOrderRequest request) => Post("/api/review-orders/create/charity", idempotencyKey, request);
        public Task<ReviewOrderDto> Pay(Guid idempotencyKey, PayReviewOrderRequest request) => Post("/api/review-orders/pay", idempotencyKey, request);

        public Task<ReviewOrderDto> AddTrackUrl(long reviewOrderId, string trackUrl, int trackDurationSeconds) => Post("/api/review-orders/track-url", new
        {
            ReviewOrderId = reviewOrderId,
            TrackUrl = trackUrl,
            TrackDurationSeconds = trackDurationSeconds,
        });

        public Task<ReviewOrderDto> TakeOrderInProgress(long reviewOrderId) => Post("/api/review-orders/take-in-progress", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Complete(long reviewOrderId, int rating) => Post("/api/review-orders/complete", new
        {
            ReviewOrderId = reviewOrderId,
            Rating = rating,
        });

        public Task<ReviewOrderDto> Freeze(long reviewOrderId) => Post("/api/review-orders/freeze", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Unfreeze(long reviewOrderId) => Post("/api/review-orders/unfreeze", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task<ReviewOrderDto> Cancel(long reviewOrderId) => Post("/api/review-orders/cancel", new
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
            requestMessage.Headers.Add(Globals.IdempotencyKey, idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request, options: serializerOptions);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            await ApiExceptionHelper.EnsureSuccessStatusCode(responseMessage);

            ReviewOrderResponse response = await responseMessage.Content.ReadFromJsonAsync<ReviewOrderResponse>(serializerOptions)
                ?? throw new InvalidOperationException("Не удалось десериализовать ReviewOrderResponse");

            return response.ReviewOrder;
        }
    }
}
