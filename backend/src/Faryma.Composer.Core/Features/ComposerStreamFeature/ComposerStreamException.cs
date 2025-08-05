using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException() : base()
        {
        }

        public ComposerStreamException(string? message) : base(message)
        {
        }

        public ComposerStreamException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}