using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderException(string message, ReviewOrder? order = null) : AppException(message)
    {
        public ReviewOrder? Order { get; } = order;

        public object GetResultObject()
        {
            if (Order is null)
            {
                return new
                {
                    ExceptionType = nameof(ReviewOrderException),
                    Message = Message,
                };
            }

            return new
            {
                ExceptionType = nameof(ReviewOrderException),
                Message = Message,
                OrderId = Order.Id,
                Status = Order.Status,
                IsFrozen = Order.IsFrozen,
            };
        }
    }
}