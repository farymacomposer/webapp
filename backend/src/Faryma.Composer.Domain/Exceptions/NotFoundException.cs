using System.Runtime.CompilerServices;

namespace Faryma.Composer.Domain.Exceptions
{
    public sealed class NotFoundException(string? message, [CallerMemberName] string callerMemberName = null!) : AppException(message, callerMemberName)
    {
    }
}
