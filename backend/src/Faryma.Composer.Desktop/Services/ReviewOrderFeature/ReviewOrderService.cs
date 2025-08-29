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
        public async Task CreateReviewOrder(Guid idempotencyKey, CreateReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");

            HttpRequestMessage requestMessage = new(HttpMethod.Post, "/api/ReviewOrder/CreateReviewOrder");
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                CreateReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<CreateReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task UpReviewOrder(Guid idempotencyKey, UpReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");

            HttpRequestMessage requestMessage = new(HttpMethod.Post, "/api/ReviewOrder/UpReviewOrder");
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString("D"));
            requestMessage.Content = JsonContent.Create(request);

            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                UpReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<UpReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
            finally
            {
                httpClient.DefaultRequestHeaders.Remove("Idempotency-Key");
            }
        }

        public async Task AddTrackUrl(AddTrackUrlRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/AddTrackUrl", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                AddTrackUrlResponse? response = await responseMessage.Content.ReadFromJsonAsync<AddTrackUrlResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task TakeOrderInProgress(TakeOrderInProgressRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/TakeOrderInProgress", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                TakeOrderInProgressResponse? response = await responseMessage.Content.ReadFromJsonAsync<TakeOrderInProgressResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task CompleteReviewOrder(CompleteReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/CompleteReviewOrder", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                CompleteReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<CompleteReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task FreezeReviewOrder(FreezeReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/FreezeReviewOrder", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                FreezeReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<FreezeReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task UnfreezeReviewOrder(UnfreezeReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/UnfreezeReviewOrder", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                UnfreezeReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<UnfreezeReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }

        public async Task CancelReviewOrder(CancelReviewOrderRequest request)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("Faryma.Composer.Api");
            HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/ReviewOrder/CancelReviewOrder", request);

            try
            {
                responseMessage.EnsureSuccessStatusCode();
                CancelReviewOrderResponse? response = await responseMessage.Content.ReadFromJsonAsync<CancelReviewOrderResponse>();
                logger.LogInformation("{@response}", response);
            }
            catch (Exception)
            {
                logger.LogError("{content}", await responseMessage.Content.ReadAsStringAsync());
            }
        }
    }
}