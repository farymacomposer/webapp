using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Desktop.Api.Exceptions;
using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;
using Faryma.Composer.Desktop.Api.ReviewOrder.Responses;
using Faryma.Composer.Desktop.Api.Shared.Dto;

namespace Faryma.Composer.Desktop.Api.ReviewOrder
{
    public sealed class ReviewOrderHttpClient(HttpClient httpClient)
    {
        private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

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

        private static async Task<ReviewOrderDto> HandleException(HttpResponseMessage responseMessage)
        {
            try
            {
                responseMessage.EnsureSuccessStatusCode();

                ReviewOrderResponse response = await responseMessage.Content.ReadFromJsonAsync<ReviewOrderResponse>()
                    ?? throw new InvalidOperationException();

                return response.ReviewOrder;
            }
            catch (HttpRequestException ex) when ((int?)ex.StatusCode == 666)
            {
                ResultObject result = await responseMessage.Content.ReadFromJsonAsync<ResultObject>()
                    ?? throw new InvalidOperationException();

                throw new ApiException(result, ex);
            }
        }

        private async Task<ReviewOrderDto> Post<T>(string requestUri, T request)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync(requestUri, request, _serializerOptions);

            return await HandleException(responseMessage);
        }

        private async Task<ReviewOrderDto> Post<T>(string requestUri, Guid idempotencyKey, T request)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, requestUri);
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request, options: _serializerOptions);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            return await HandleException(responseMessage);
        }
    }
}