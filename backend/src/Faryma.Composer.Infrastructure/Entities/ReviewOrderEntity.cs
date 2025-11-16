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
    public sealed class ReviewOrderEntity : BaseEntity
    {
        /// <summary>
        /// Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа
        /// </summary>
        public required string MainNickname { get; set; }
        public required string MainNormalizedNickname { get; set; }

        public long CreationStreamId { get; set; }

        /// <summary>
        /// Дата и время создания заказа
        /// </summary>
        public required DateTime CreatedAt { get; set; }

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
        /// Тип заказа
        /// </summary>
        [Column(TypeName = DbContextHelper.ReviewOrderTypeEnum)]
        public required ReviewOrderType Type { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        [Column(TypeName = DbContextHelper.ReviewOrderStatusEnum)]
        public required ReviewOrderStatus Status { get; set; }

        /// <summary>
        /// Тип категории заказа
        /// </summary>
        [Column(TypeName = DbContextHelper.OrderCategoryTypeEnum)]
        public required OrderCategoryType CategoryType { get; set; }

        /// <summary>
        /// Заказ заморожен
        /// </summary>
        public required bool IsFrozen { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; set; }

        public long? TrackId { get; set; }

        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required decimal NominalAmount { get; set; }

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
        [ForeignKey(nameof(TrackId))]
        public TrackEntity? Track { get; set; }

        /// <summary>
        /// Связанный cтрим, где создан заказ
        /// </summary>
        [ForeignKey(nameof(CreationStreamId))]
        public required ComposerStreamEntity CreationStream { get; set; }

        /// <summary>
        /// Связанный cтрим, где заказ взят в работу
        /// </summary>
        [ForeignKey(nameof(ProcessingStreamId))]
        public ComposerStreamEntity? ProcessingStream { get; set; }

        /// <summary>
        /// Пользователь или пользователи, создавшие заказ
        /// </summary>
        public ICollection<UserNicknameEntity> UserNicknames { get; set; } = [];

        /// <summary>
        /// Платежи
        /// </summary>
        public ICollection<TransactionEntity> Payments { get; set; } = [];

        /// <summary>
        /// Возвращает общую стоимость заказа
        /// </summary>
        public decimal GetTotalAmount()
        {
            return Type switch
            {
                ReviewOrderType.OutOfQueue => 0,
                ReviewOrderType.Donation => Payments.Sum(x => x.Amount),
                ReviewOrderType.Free => NominalAmount + Payments.Sum(x => x.Amount),
                ReviewOrderType.Charity => 0,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}