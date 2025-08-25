using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueException : AppException
    {
        public OrderQueueException(string? message) : base(message)
        {
        }
    }
}