using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;

namespace Faryma.Composer.Desktop.Api.ReviewOrder
{
    public sealed class ReviewOrderHttpClient(HttpClient httpClient)
    {
        private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public Task Create(Guid idempotencyKey, CreateReviewOrderRequest request) => Post("/api/ReviewOrder/CreateReviewOrder", idempotencyKey, request);
        public Task MoveUp(Guid idempotencyKey, MoveUpReviewOrderRequest request) => Post("/api/ReviewOrder/MoveUpReviewOrder", idempotencyKey, request);

        public Task AddTrackUrl(long reviewOrderId, string trackUrl) => Post("/api/ReviewOrder/AddTrackUrl", new
        {
            ReviewOrderId = reviewOrderId,
            TrackUrl = trackUrl,
        });

        public Task TakeOrderInProgress(long reviewOrderId) => Post("/api/ReviewOrder/TakeOrderInProgress", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task Complete(long reviewOrderId, int rating) => Post("/api/ReviewOrder/CompleteReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
            Rating = rating,
        });

        public Task Freeze(long reviewOrderId) => Post("/api/ReviewOrder/FreezeReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task Unfreeze(long reviewOrderId) => Post("/api/ReviewOrder/UnfreezeReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        public Task Cancel(long reviewOrderId) => Post("/api/ReviewOrder/CancelReviewOrder", new
        {
            ReviewOrderId = reviewOrderId,
        });

        private async Task Post<T>(string requestUri, T request)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync(requestUri, request, _serializerOptions);
            responseMessage.EnsureSuccessStatusCode();
        }

        private async Task Post<T>(string requestUri, Guid idempotencyKey, T request)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, requestUri);
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request, options: _serializerOptions);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
        }
    }
}