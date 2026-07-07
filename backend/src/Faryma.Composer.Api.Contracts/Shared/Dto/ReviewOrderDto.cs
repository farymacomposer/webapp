using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Api.Contracts.Shared.Dto
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    public sealed record ReviewOrderDto
    {
        /// <summary>
        /// Id заказа
        /// </summary>
        public required long Id { get; init; }

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public required DateTime? InProgressAt { get; init; }

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public required DateTime? CompletedAt { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; init; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public required ReviewOrderStatus Status { get; init; }

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public required bool IsFrozen { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        public required int? TrackDurationSeconds { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public required string? UserComment { get; init; }

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public required string MainNickname { get; init; }

        /// <summary>
        /// Денежная сумма для обратной совместимости. Совпадает с PaidPriorityAmount.
        /// </summary>
        public required long TotalAmount { get; init; }

        /// <summary>
        /// Обязательная стоимость заказа
        /// </summary>
        public required long RequiredAmount { get; init; }

        /// <summary>
        /// Сумма покрытия обязательной стоимости
        /// </summary>
        public required long CoveredAmount { get; init; }

        /// <summary>
        /// Сумма денежных платежей по заказу
        /// </summary>
        public required long PaidAmount { get; init; }

        /// <summary>
        /// Денежная сумма, которая влияет на донатный приоритет
        /// </summary>
        public required long PaidPriorityAmount { get; init; }

        /// <summary>
        /// Связанный cтрим композитора, где создан заказ
        /// </summary>
        [Required]
        public required ComposerStreamDto CreationStream { get; init; }

        public static ReviewOrderDto Map(
            ReviewOrderEntity item,
            long requiredAmount,
            long coveredAmount,
            long paidAmount,
            long paidPriorityAmount)
        {
            return new()
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                InProgressAt = item.InProgressAt,
                CompletedAt = item.CompletedAt,
                Type = item.Type,
                Status = item.Status,
                IsFrozen = item.IsFrozen,
                TrackUrl = item.TrackUrl,
                TrackDurationSeconds = item.TrackDurationSeconds,
                UserComment = item.UserComment,
                MainNickname = item.MainNickname,
                TotalAmount = paidPriorityAmount,
                RequiredAmount = requiredAmount,
                CoveredAmount = coveredAmount,
                PaidAmount = paidAmount,
                PaidPriorityAmount = paidPriorityAmount,
                CreationStream = ComposerStreamDto.Map(item.CreationStream),
            };
        }

        public static ReviewOrderDto Map(ReviewOrderEntity item)
        {
            long paidAmount = GetPaymentAmount(item.Transactions);
            long paidPriorityAmount = item.Type switch
            {
                ReviewOrderType.Donation or ReviewOrderType.Free => paidAmount,
                ReviewOrderType.OutOfQueue or ReviewOrderType.Charity => 0,
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new InvalidOperationException("Неподдерживаемый тип заказа"),
            };

            return Map(
                item,
                item.PayableAmount,
                paidAmount + (item.CoverageRedemption?.CoveredAmount ?? 0),
                paidAmount,
                paidPriorityAmount);
        }

        private static long GetPaymentAmount(IEnumerable<TransactionEntity>? transactions)
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
