using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueException : AppException
    {
        public OrderQueueException() : base()
        {
        }

        public OrderQueueException(string? message) : base(message)
        {
        }

        public OrderQueueException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}