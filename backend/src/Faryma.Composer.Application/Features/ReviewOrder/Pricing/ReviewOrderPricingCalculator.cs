using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.ReviewOrder.Pricing
{
    /// <summary>
    /// Общие методы расчета денежных сумм заказа разбора
    /// </summary>
    public static class ReviewOrderPricingCalculator
    {
        /// <summary>
        /// Рассчитывает сумму денежных платежей по заказу без отдельных услуг
        /// </summary>
        public static long CalculateOrderPaymentAmount(ReviewOrderEntity order) => CalculatePaymentAmount(order.Transactions);

        /// <summary>
        /// Рассчитывает денежную сумму заказа, которая влияет на донатный приоритет
        /// </summary>
        public static long CalculatePaidPriorityAmount(ReviewOrderEntity order)
        {
            return order.Type switch
            {
                ReviewOrderType.Donation or ReviewOrderType.Free => CalculateOrderPaymentAmount(order),
                ReviewOrderType.OutOfQueue or ReviewOrderType.Charity => 0,
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new InvalidOperationException("Неподдерживаемый тип заказа"),
            };
        }

        /// <summary>
        /// Рассчитывает сумму платежей в наборе транзакций
        /// </summary>
        public static long CalculatePaymentAmount(IEnumerable<TransactionEntity>? transactions)
        {
            if (transactions is null)
            {
                return 0;
            }

            return transactions
                .Where(x => x.Kind == TransactionKind.Payment)
                .Sum(x => x.Debit);
        }
    }
}
