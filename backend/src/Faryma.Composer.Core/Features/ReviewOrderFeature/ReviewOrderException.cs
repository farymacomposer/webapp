using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderException : AppException
    {
        public ReviewOrderException(string message, ReviewOrder? order = null, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
            if (order is not null)
            {
                Data.Add("Id", order.Id);
                Data.Add("Status", order.Status.ToString());
                Data.Add("IsFrozen", order.IsFrozen);
            }
        }
    }
}