using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ReviewFeature
{
    public sealed class ReviewException : AppException
    {
        public ReviewException() : base()
        {
        }

        public ReviewException(string? message) : base(message)
        {
        }

        public ReviewException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}