using System.Runtime.CompilerServices;
using Faryma.Composer.Contracts.Exceptions;
using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Contracts.Application.Features.ReviewOrder
{
    public sealed class ReviewOrderException : AppException
    {
        public ReviewOrderException(string message, ReviewOrderEntity? order = null, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
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