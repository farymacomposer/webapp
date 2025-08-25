using System.Runtime.CompilerServices;

namespace Faryma.Composer.Infrastructure.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string? message, long id, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
            Data.Add("Id", id);
        }
    }
}