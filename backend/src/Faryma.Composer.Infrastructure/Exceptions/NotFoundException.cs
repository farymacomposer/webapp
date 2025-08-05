namespace Faryma.Composer.Infrastructure.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException() : base()
        {
        }

        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}