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
            ReviewOrder order = await ofw.ReviewOrderRepository.Find(command.ReviewOrderId)
                ?? throw new ReviewException($"Заказ разбора трека Id: {command.ReviewOrderId}, не существует");

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewException($"Невозможно оценить заказ в статусе '{order.Status}'");
            }

            Review review = ofw.ReviewRepository.Create(
                command.Comment,
                order.ComposerStream,
                order,
                command.Rating,
                order.TrackUrl!);

            order.Status = ReviewOrderStatus.Completed;
            order.Review = review;

            await ofw.SaveChangesAsync();

            await orderQueueService.CompleteReview(order);

            return review;
        }
    }
}