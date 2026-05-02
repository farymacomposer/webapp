using System.Diagnostics;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    [DebuggerDisplay("MainNickname = {MainNickname}")]
    public sealed class ReviewOrderEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public required string MainNickname { get; set; }

        public required string MainNormalizedNickname { get; set; }

        public long CreationStreamId { get; set; }

        public long? ProcessingStreamId { get; set; }

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public DateTime? InProgressAt { get; set; }

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Дата и время отмены заказа
        /// </summary>
        public DateTime? CanceledAt { get; set; }

        /// <summary>
        /// Причина отмены заказа
        /// </summary>
        public string? CancelReason { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public required ReviewOrderStatus Status { get; set; }

        /// <summary>
        /// Категория заказа в очереди (записывается при взятии заказа в работу)
        /// </summary>
        public required QueueCategory QueueCategory { get; set; }

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public required bool IsFrozen { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; set; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        public int? TrackDurationSeconds { get; set; }

        public long? TrackId { get; set; }

        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required long NominalAmount { get; set; }

        /// <summary>
        /// Сумма к оплате
        /// </summary>
        public required long PayableAmount { get; set; }

        /// <summary>
        /// Комментарий к цене
        /// </summary>
        public string? PricingComment { get; set; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Результат разбора
        /// </summary>
        public ReviewEntity? Review { get; set; }

        /// <summary>
        /// Связанный музыкальный трек
        /// </summary>
        public TrackEntity? Track { get; set; }

        /// <summary>
        /// Связанный cтрим, где создан заказ
        /// </summary>
        public required ComposerStreamEntity CreationStream { get; set; }

        /// <summary>
        /// Связанный cтрим, где заказ взят в работу
        /// </summary>
        public ComposerStreamEntity? ProcessingStream { get; set; }

        /// <summary>
        /// Пользователь или пользователи, создавшие заказ
        /// </summary>
        public ICollection<UserNicknameEntity> UserNicknames { get; set; } = [];

        /// <summary>
        /// Возвращает общую стоимость заказа
        /// </summary>
        public long GetTotalAmount()
        {
            long result = Type switch
            {
                ReviewOrderType.OutOfQueue => 0,
                ReviewOrderType.Donation => Transactions.Where(x => x.Kind == TransactionKind.Payment).Sum(x => x.Debit),
                ReviewOrderType.Free => NominalAmount + Transactions.Where(x => x.Kind == TransactionKind.Payment).Sum(x => x.Debit),
                ReviewOrderType.Charity => 0,
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new UnreachableException("Неподдерживаемый тип заказа"),
            };

            return result;
        }
    }
}
