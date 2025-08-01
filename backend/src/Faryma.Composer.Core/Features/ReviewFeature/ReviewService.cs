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
                throw new ReviewException("Невозможно оценить заказ т.к. его статус не в процессе");
            }

            Review review = await CreateReview(command, order);

            order.Status = ReviewOrderStatus.Completed;
            order.Review = review;

            await ofw.SaveChangesAsync();

            await orderQueueService.CompleteReview(order);

            return review;
        }

        private async Task<Review> CreateReview(CompleteReviewCommand command, ReviewOrder order)
        {
            Track track;

            if (order.TrackId.HasValue)
            {
                track = await ofw.TrackRepository.Find(order.TrackId.Value)
                    ?? throw new ReviewException($"Трека {order.TrackId.Value} не существует");
            }
            else if (!string.IsNullOrWhiteSpace(order.TrackUrl))
            {
                UserNickname userNickname = order.UserNicknames.First();
                track = await ofw.TrackRepository.GetOrCreateByUrl(userNickname, order.TrackUrl);
            }
            else
            {
                throw new ReviewException($"У заказа {order.Id} нет трека");
            }

            Review trackReview = new()
            {
                Comment = command.Comment,
                CompletedAt = DateTime.UtcNow,
                ComposerStream = order.ComposerStream,
                ReviewOrder = order,

                //TODO: Узнать макс мин значения рейтинга и добавить валидацию
                Rating = command.Rating,

                Track = track,
                UpdatedAt = DateTime.UtcNow,
            };

            ofw.ReviewRepository.Add(trackReview);

            return trackReview;
        }
    }
}