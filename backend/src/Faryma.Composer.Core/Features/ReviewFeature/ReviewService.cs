using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.ReviewFeature.Commands;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ReviewFeature
{
    public sealed class ReviewService(
        UnitOfWork ofw,
        OrderQueueService orderQueueService)
    {
        public async Task<Review> CompleteReview(CompleteReviewCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewException($"Невозможно выполнить заказ в статусе '{order.Status}'");
            }

            ComposerStream? liveStream = await ofw.ComposerStreamRepository.FindLiveStream()
                ?? throw new ReviewException("Невозможно выполнить заказ вне активного стрима");

            order.Review = ofw.ReviewRepository.Create(order, liveStream, command.Rating, command.Comment);
            order.Status = ReviewOrderStatus.Completed;

            await ofw.SaveChangesAsync();

            await orderQueueService.CompleteReview(order);

            return order.Review;
        }
    }
}