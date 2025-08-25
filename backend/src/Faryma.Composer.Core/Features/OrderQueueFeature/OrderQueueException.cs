using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueException : AppException
    {
        public OrderQueueException(string? message, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
        }
    }
}