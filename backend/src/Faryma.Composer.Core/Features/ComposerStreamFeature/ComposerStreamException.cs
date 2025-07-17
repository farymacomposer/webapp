namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamException : Exception
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