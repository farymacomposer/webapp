using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderException(string message, ReviewOrder? order = null) : AppException(message)
    {
        public ReviewOrder? Order { get; } = order;
    }
}