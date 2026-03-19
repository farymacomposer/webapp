using System.Diagnostics;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.OrderQueue.PriorityAlgorithm
{
    /// <summary>
    /// Состояние менеджера приоритетов активных заказов
    /// </summary>
    public sealed class OrderPriorityManagerState
    {
        /// <summary>
        /// Последнее состояние менеджера приоритетов
        /// </summary>
        public required CategoryState LastPriorityManagerState { get; set; }

        /// <summary>
        /// Последний обработанный никнейм
        /// </summary>
        public required string? LastIssuedNickname { get; set; }

        /// <summary>
        /// Последний никнейм в категории - вне очереди
        /// </summary>
        public required string? LastOutOfQueueNickname { get; set; }

        /// <summary>
        /// Последний никнейм в донатной и долговых категориях (по дате стрима)
        /// </summary>
        public required Dictionary<DateOnly, string> LastNicknamesByStreamDate { get; init; }

        /// <summary>
        /// Последняя дата долговой категории
        /// </summary>
        public required DateOnly? LastDebtCategoryDate { get; set; }

        public static CategoryState MapCategoryState(OrderCategoryType categoryType)
        {
            return categoryType switch
            {
                OrderCategoryType.OutOfQueue => CategoryState.OutOfQueue,
                OrderCategoryType.Donation => CategoryState.Donation,
                OrderCategoryType.Debt => CategoryState.Debt,
                _ => throw new UnreachableException($"Неподдерживаемый тип категории заказа '{categoryType}'")
            };
        }

        /// <summary>
        /// Обновляет состояние менеджера приоритетов
        /// </summary>
        public void UpdateFromOrder(ReviewOrderEntity order)
        {
            LastPriorityManagerState = MapCategoryState(order.CategoryType);
            LastIssuedNickname = order.MainNormalizedNickname;

            if (order.Type == ReviewOrderType.OutOfQueue)
            {
                LastOutOfQueueNickname = order.MainNormalizedNickname;
            }
            else
            {
                if (order.CategoryType == OrderCategoryType.Debt)
                {
                    LastDebtCategoryDate = order.CreationStream.EventDate;
                }

                DateOnly streamDate = order.CreationStream.EventDate;
                LastNicknamesByStreamDate[streamDate] = order.MainNormalizedNickname;
            }
        }
    }
}