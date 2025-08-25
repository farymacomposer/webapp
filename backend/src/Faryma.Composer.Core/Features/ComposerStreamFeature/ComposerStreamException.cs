using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException(string? message, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
        }
    }
}