using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Заказ разбора трека
    /// </summary>
    [DebuggerDisplay("MainNickname = {MainNickname}")]
    [Table("review_orders")]
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
        /// Тип заказа
        /// </summary>
        [Column(TypeName = DbEnumConst.ReviewOrderTypeEnum)]
        public required ReviewOrderType Type { get; set; }

        /// <summary>
        /// Статус заказа
        /// </summary>
        [Column(TypeName = DbEnumConst.ReviewOrderStatusEnum)]
        public required ReviewOrderStatus Status { get; set; }

        /// <summary>
        /// Тип категории заказа (записывается при взятии заказа в работу)
        /// </summary>
        [Column(TypeName = DbEnumConst.OrderCategoryTypeEnum)]
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
        public required long NominalAmount { get; set; }

        /// <summary>
        /// Сумма к оплате
        /// </summary>
        public required long PayableAmount { get; set; }

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
        /// Возвращает общую стоимость заказа
        /// </summary>
        public long GetTotalAmount()
        {
            return Type switch
            {
                ReviewOrderType.OutOfQueue => 0,
                ReviewOrderType.Donation => Transactions.Where(x => x.Kind == TransactionKind.Payment).Sum(x => x.Debit),
                ReviewOrderType.Free => NominalAmount + Transactions.Where(x => x.Kind == TransactionKind.Payment).Sum(x => x.Debit),
                ReviewOrderType.Charity => 0,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}