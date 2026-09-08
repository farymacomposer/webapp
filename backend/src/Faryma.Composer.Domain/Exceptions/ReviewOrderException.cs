using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Domain.Exceptions
{
    public sealed class ReviewOrderException : AppException
    {
        public ReviewOrderException(string? message, ReviewOrderEntity? order = null) : base(message)
        {
            if (order is not null)
            {
                Context = new
                {
                    Id = order.Id,
                    Status = order.Status,
                    IsFrozen = order.IsFrozen,
                };
            }
        }
    }
}
