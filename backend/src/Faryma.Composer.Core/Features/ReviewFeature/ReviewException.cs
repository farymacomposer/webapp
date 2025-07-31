namespace Faryma.Composer.Core.Features.ReviewFeature
{
    public sealed class ReviewException : Exception
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