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
    public sealed class ReviewController(ReviewService reviewService) : ControllerBase
    {

        /// <summary>
        /// Завершает разбор трека
        /// </summary>
        /// <param name="request">Запрос завершения разбора</param>
        [HttpPost(nameof(CompleteReview))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CompleteReviewResponse>> CompleteReview(
            [FromBody] CompleteReviewRequest request)
        {
            Review review = await reviewService.CompleteReview(request.Map());

            CompleteReviewResponse response = new()
            {
                ReviewOrderId = request.ReviewOrderId,
                ReviewId = review.Id
            };

            return Ok(response);
        }
    }
}