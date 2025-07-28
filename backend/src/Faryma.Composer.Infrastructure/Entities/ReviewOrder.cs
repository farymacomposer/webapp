using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Faryma.Composer.Infrastructure.Abstractions;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    [DebuggerDisplay("Nickname = {UserNickname.Nickname}")]
    public sealed class ReviewOrder : BaseEntity
    {
        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время начала разбора трека
        /// </summary>
        public DateTime? InProgressAt { get; set; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType Type { get; set; }

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

        public long? TrackId { get; set; }
        public Guid UserNicknameId { get; set; }
        public long ComposerStreamId { get; set; }

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
        /// Пользователь, создавший заказ
        /// </summary>
        [ForeignKey(nameof(UserNicknameId))]
        public required UserNickname UserNickname { get; set; }

        /// <summary>
        /// Связанный cтрим композитора
        /// </summary>
        [ForeignKey(nameof(ComposerStreamId))]
        public required ComposerStream ComposerStream { get; set; }

        /// <summary>
        /// Платежи
        /// </summary>
        public ICollection<Transaction> Payments { get; set; } = [];
    }
}