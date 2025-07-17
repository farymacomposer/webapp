using Faryma.Composer.Api.Features.ReviewOrderFeature.Create;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Up;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature
{
    public static class Mapper
    {
        public static CreateCommand Map(CreateReviewOrderRequest item)
        {
            return new()
            {
                Nickname = item.Nickname,
                OrderType = item.OrderType,
                PaymentAmount = item.PaymentAmount,
                TrackUrl = item.TrackUrl,
                UserComment = item.UserComment,
            };
        }

        public static UpCommand Map(UpReviewOrderRequest item)
        {
            return new()
            {
                Nickname = item.Nickname,
                ReviewOrderId = item.ReviewOrderId,
                PaymentAmount = item.PaymentAmount,
            };
        }
    }
}