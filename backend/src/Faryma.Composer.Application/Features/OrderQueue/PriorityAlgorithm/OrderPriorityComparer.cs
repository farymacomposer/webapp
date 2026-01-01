# nullable disable
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;

namespace Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm
{
    /// <summary>
    /// Базовая приоритезация для заказов, по сумме и по дате
    /// </summary>
    public sealed class OrderPriorityComparer : IComparer<ReviewOrderEntity>
    {
        public static OrderPriorityComparer Default { get; } = new();

        public int Compare(ReviewOrderEntity x, ReviewOrderEntity y)
        {
            decimal xAmount = x.GetTotalAmount();
            decimal yAmount = y.GetTotalAmount();

            int result = decimal.Compare(xAmount, yAmount) * -1;
            if (result != 0)
            {
                return result;
            }

            return DateTime.Compare(x.CreatedAt, y.CreatedAt);
        }
    }
}