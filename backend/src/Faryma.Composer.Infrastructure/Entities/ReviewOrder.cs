using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    [DebuggerDisplay("MainNickname = {MainNickname}")]
    public sealed class ReviewOrder : BaseEntity
    {
        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время взятия заказа в работу
        /// </summary>
        public DateTime? InProgressAt { get; set; }

        /// <summary>
        /// Дата и время выполнения заказа
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; set; }

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        public required OrderCategoryType CategoryType { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        public required ReviewOrderStatus Status { get; set; }

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public required bool IsFrozen { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; set; }

        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных)
        /// </summary>
        public decimal NominalAmount { get; set; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; set; }

        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public required string MainNickname { get; set; }
        public required string MainNormalizedNickname { get; set; }

        public long? TrackId { get; set; }
        public long CreationStreamId { get; set; }
        public long? ProcessingStreamId { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Результат разбора
        /// </summary>
        public Review? Review { get; set; }

        /// <summary>
        /// Трек для разбора
        /// </summary>
        [ForeignKey(nameof(TrackId))]
        public Track? Track { get; set; }

        /// <summary>
        /// Связанный cтрим композитора, где создан заказ
        /// </summary>
        [ForeignKey(nameof(CreationStreamId))]
        public required ComposerStream CreationStream { get; set; }

        /// <summary>
        /// Связанный cтрим композитора, где заказ взят в работу
        /// </summary>
        [ForeignKey(nameof(ProcessingStreamId))]
        public ComposerStream? ProcessingStream { get; set; }

        /// <summary>
        /// Пользователь или пользователи, создавшие заказ
        /// </summary>
        public ICollection<UserNickname> UserNicknames { get; set; } = [];

        /// <summary>
        /// Платежи
        /// </summary>
        public ICollection<Transaction> Payments { get; set; } = [];

        /// <summary>
        /// Возвращает общую стоимость заказа
        /// </summary>
        public decimal GetTotalAmount() => NominalAmount + Payments.Sum(x => x.Amount);

        public override int GetHashCode() => HashCode.Combine(
            Id,
            CategoryType,
            Status,
            IsFrozen,
            TrackUrl,
            CreationStream.Status,
            GetTotalAmount());
    }
}