using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Application.Features.OrderQueueFeature
{
    public sealed class OrderQueueException(string? message, [CallerMemberName] string callerMemberName = null!) : AppException(message, callerMemberName)
    {
    }
}