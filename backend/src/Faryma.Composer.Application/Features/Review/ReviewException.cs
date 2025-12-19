using System.Runtime.CompilerServices;
using Faryma.Composer.Contracts.Exceptions;

namespace Faryma.Composer.Application.Features.ReviewFeature
{
    public sealed class ReviewException : AppException
    {
        public ReviewException(string? message, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
        }
    }
}