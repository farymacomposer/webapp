namespace Faryma.Composer.Infrastructure.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException() : base()
        {
        }

        protected AppException(string? message) : base(message)
        {
        }

        protected AppException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}