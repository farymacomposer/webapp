using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException(string? message) : base(message)
        {
        }
    }
}