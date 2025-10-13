using System.Net.Http.Json;
using Faryma.Composer.Desktop.Api.ReviewOrder.Requests;

namespace Faryma.Composer.Desktop.Api.ReviewOrder
{
    public sealed class ReviewOrderHttpClient(HttpClient httpClient)
    {
        public async Task Create(Guid idempotencyKey, CreateReviewOrderRequest request)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, "/api/ReviewOrder/CreateReviewOrder");
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task MoveUp(Guid idempotencyKey, MoveUpReviewOrderRequest request)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Post, "/api/ReviewOrder/MoveUpReviewOrder");
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task AddTrackUrl(long reviewOrderId, string trackUrl)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/AddTrackUrl", new
            {
                ReviewOrderId = reviewOrderId,
                TrackUrl = trackUrl,
            });

            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task TakeOrderInProgress(long reviewOrderId)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/TakeOrderInProgress", new
            {
                ReviewOrderId = reviewOrderId,
            });

            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task Complete(long reviewOrderId, int rating)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/CompleteReviewOrder", new
            {
                ReviewOrderId = reviewOrderId,
                Rating = rating,
            });

            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task Freeze(long reviewOrderId)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/FreezeReviewOrder", new
            {
                ReviewOrderId = reviewOrderId,
            });

            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task Unfreeze(long reviewOrderId)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/UnfreezeReviewOrder", new
            {
                ReviewOrderId = reviewOrderId,
            });

            responseMessage.EnsureSuccessStatusCode();
        }

        public async Task Cancel(long reviewOrderId)
        {
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/CancelReviewOrder", new
            {
                ReviewOrderId = reviewOrderId,
            });

            responseMessage.EnsureSuccessStatusCode();
        }
    }
}