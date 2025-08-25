using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueException(string? message, [CallerMemberName] string callerMemberName = null!) : AppException(message, callerMemberName)
    {
    }
}