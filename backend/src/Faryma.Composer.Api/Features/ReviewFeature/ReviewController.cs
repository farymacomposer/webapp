using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.ReviewFeature.Complete;
using Faryma.Composer.Core.Features.ReviewFeature;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Faryma.Composer.Api.Features.ReviewFeature
{
    /// <summary>
    /// Управление разборами треков
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ReviewController(ReviewService reviewService, IMemoryCache cache) : ControllerBase
    {
        private static readonly TimeSpan _idempotencyKeyExpiration = TimeSpan.FromHours(1);

        /// <summary>
        /// Завершает разбор трека
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос завершения разбора</param>
        [HttpPost(nameof(CompleteReview))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CompleteReviewResponse>> CompleteReview(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] CompleteReviewRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(CompleteReview)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out CompleteReviewResponse? cachedResponse))
            {
                return Ok(cachedResponse);
            }

            Review review = await reviewService.CompleteReview(request.Map());

            CompleteReviewResponse response = new()
            {
                ReviewOrderId = request.ReviewOrderId,
                ReviewId = review.Id
            };

            cache.Set(key, response, _idempotencyKeyExpiration);

            return Ok(response);
        }
    }
}