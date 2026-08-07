# nullable disable
using Faryma.Composer.Domain.Entities.TransactionSources;

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
            long xAmount = x.GetPaidPriorityAmount();
            long yAmount = y.GetPaidPriorityAmount();

            int result = decimal.Compare(xAmount, yAmount) * -1;
            if (result != 0)
            {
                return result;
            }

            return DateTime.Compare(x.CreatedAt, y.CreatedAt);
        }
    }
}
