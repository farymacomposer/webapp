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

        public static CategoryState MapCategoryState(QueueCategory? queueCategory)
        {
            if (!queueCategory.HasValue)
            {
                throw new UnreachableException("Категория заказа не определена");
            }

            return queueCategory.Value switch
            {
                QueueCategory.OutOfQueue => CategoryState.OutOfQueue,
                QueueCategory.Donation => CategoryState.Donation,
                QueueCategory.Debt => CategoryState.Debt,
                _ => throw new UnreachableException($"Неподдерживаемая категория заказа '{queueCategory}'")
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
