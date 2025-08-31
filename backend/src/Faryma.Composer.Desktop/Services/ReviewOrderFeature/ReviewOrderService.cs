using System.Net.Http.Json;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.AddTrackUrl;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Cancel;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Complete;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Dto;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Freeze;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.TakeInProgress;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Unfreeze;
using Faryma.Composer.Desktop.Services.ReviewOrderFeature.Up;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature
{
    public sealed class ReviewOrderService(IHttpClientFactory httpClientFactory, ILogger<ReviewOrderService> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient("Faryma.Composer.Api");

        public async Task Post<T>(Guid idempotencyKey, T request)
        {
            string requestUri = request switch
            {
                CreateReviewOrderRequest => "/api/ReviewOrder/CreateReviewOrder",
                UpReviewOrderRequest => "/api/ReviewOrder/UpReviewOrder",
                _ => throw new InvalidOperationException()
            };

            HttpRequestMessage requestMessage = new(HttpMethod.Post, requestUri);
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request);

            HttpResponseMessage responseMessage = await HttpClient.SendAsync(requestMessage);
            await LogError(responseMessage);
        }

        public async Task Post<T>(T request)
        {
            string requestUri = request switch
            {
                AddTrackUrlRequest => "/api/ReviewOrder/AddTrackUrl",
                TakeOrderInProgressRequest => "/api/ReviewOrder/TakeOrderInProgress",
                CompleteReviewOrderRequest => "/api/ReviewOrder/CompleteReviewOrder",
                FreezeReviewOrderRequest => "/api/ReviewOrder/FreezeReviewOrder",
                UnfreezeReviewOrderRequest => "/api/ReviewOrder/UnfreezeReviewOrder",
                CancelReviewOrderRequest => "/api/ReviewOrder/CancelReviewOrder",
                _ => throw new InvalidOperationException()
            };

            HttpResponseMessage responseMessage = await HttpClient.PostAsJsonAsync(requestUri, request);
            await LogError(responseMessage);
        }

        private async Task LogError(HttpResponseMessage responseMessage)
        {
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