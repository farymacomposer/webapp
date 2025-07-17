namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderException : Exception
    {
        public ReviewOrderException() : base()
        {
        }

        public ReviewOrderException(string? message) : base(message)
        {
        }

        public ReviewOrderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}