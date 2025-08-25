using System.Runtime.CompilerServices;

namespace Faryma.Composer.Infrastructure.Exceptions
{
    public sealed class NotFoundException(string? message, [CallerMemberName] string callerMemberName = null!) : AppException(message, callerMemberName)
    {
    }
}