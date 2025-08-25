using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ReviewFeature
{
    public sealed class ReviewException : AppException
    {
        public ReviewException(string? message) : base(message)
        {
        }
    }
}