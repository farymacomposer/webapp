# nullable disable
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm
{
    /// <summary>
    /// Базовая приоритезация для заказов, по сумме и по дате
    /// </summary>
    public sealed class OrderPriorityComparer : IComparer<ReviewOrder>
    {
        public static OrderPriorityComparer Default { get; } = new();

        public int Compare(ReviewOrder x, ReviewOrder y)
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