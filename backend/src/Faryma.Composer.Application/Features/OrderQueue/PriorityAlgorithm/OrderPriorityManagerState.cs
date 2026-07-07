using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

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

        public static CategoryState MapCategoryState(QueueCategory queueCategory)
        {
            return queueCategory switch
            {
                QueueCategory.OutOfQueue => CategoryState.OutOfQueue,
                QueueCategory.Donation => CategoryState.Donation,
                QueueCategory.Debt => CategoryState.Debt,
                _ => throw new ArgumentException($"Неподдерживаемая категория заказа '{queueCategory}'", nameof(queueCategory))
            };
        }

        /// <summary>
        /// Обновляет состояние менеджера приоритетов
        /// </summary>
        public void UpdateFromOrder(ReviewOrderEntity order)
        {
            LastPriorityManagerState = MapCategoryState(order.QueueCategory);
            LastIssuedNickname = order.MainNormalizedNickname;

            if (order.Type == ReviewOrderType.OutOfQueue)
            {
                LastOutOfQueueNickname = order.MainNormalizedNickname;
            }
            else
            {
                if (order.QueueCategory == QueueCategory.Debt)
                {
                    LastDebtCategoryDate = order.CreationStream.EventDate;
                }

                DateOnly streamDate = order.CreationStream.EventDate;
                LastNicknamesByStreamDate[streamDate] = order.MainNormalizedNickname;
            }
        }
    }
}
