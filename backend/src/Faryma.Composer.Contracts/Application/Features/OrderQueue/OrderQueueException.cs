using System.Runtime.CompilerServices;
using Faryma.Composer.Contracts.Exceptions;

namespace Faryma.Composer.Contracts.Application.Features.OrderQueue
{
    public sealed class OrderQueueException(string? message, [CallerMemberName] string callerMemberName = null!) : AppException(message, callerMemberName)
    {
    }
}