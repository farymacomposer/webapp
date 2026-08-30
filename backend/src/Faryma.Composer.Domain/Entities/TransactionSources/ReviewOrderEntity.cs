using System.Diagnostics;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Domain.Entities.TransactionSources
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
        /// Стоимость заказа
        /// </summary>
        public required long Price { get; set; }

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
        /// Оплата подробного разбора
        /// </summary>
        public ReviewOrderDetailedReviewPaymentEntity? DetailedReviewPayment { get; set; }

        /// <summary>
        /// Погашение жетона, дающее право на этот заказ
        /// </summary>
        public UserEntitlementRedemptionEntity? CoverageRedemption { get; set; }

        public void ThrowIfCannotBePaid()
        {
            if (Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.AwaitingPayment or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно оплатить заказ", this);
            }

            if (Type is not (ReviewOrderType.Donation or ReviewOrderType.Free))
            {
                throw new ReviewOrderException("Тип заказа не поддерживает денежную оплату", this);
            }
        }

        public void Pay(long requiredAmount)
        {
            ThrowIfCannotBePaid();

            RecalculatePaymentState(requiredAmount);
        }

        public void ThrowIfCannotBeAddTrackUrl()
        {
            if (Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.AwaitingPayment or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно добавить/изменить ссылку на трек", this);
            }
        }

        public void AddTrackUrl(string trackUrl, int trackDurationSeconds, long requiredAmount)
        {
            ThrowIfCannotBeAddTrackUrl();

            TrackUrl = trackUrl;
            TrackDurationSeconds = trackDurationSeconds;
            RecalculatePaymentState(requiredAmount);
        }

        public void ThrowIfCannotBeTakeInProgress()
        {
            if (IsFrozen || Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException("Невозможно взять в работу заказ", this);
            }
        }

        public void TakeInProgress(ComposerStreamEntity liveStream, QueueCategory queueCategory, DateTime now)
        {
            ThrowIfCannotBeTakeInProgress();

            QueueCategory = queueCategory;
            ProcessingStream = liveStream;
            Status = ReviewOrderStatus.InProgress;
            InProgressAt = now;
        }

        public void ThrowIfCannotBeComplete()
        {
            if (Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewOrderException("Невозможно выполнить заказ", this);
            }
        }

        public void Complete(ReviewEntity review, DateTime now)
        {
            ThrowIfCannotBeComplete();

            Review = review;
            CompletedAt = now;
            Status = ReviewOrderStatus.Completed;
        }

        public void ThrowIfCannotBeFreeze()
        {
            if (Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно заморозить заказ", this);
            }
        }

        public void Freeze()
        {
            ThrowIfCannotBeFreeze();

            IsFrozen = true;
        }

        public void ThrowIfCannotBeUnfreeze()
        {
            if (Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно разморозить заказ", this);
            }
        }

        public void Unfreeze()
        {
            ThrowIfCannotBeUnfreeze();

            IsFrozen = false;
        }

        public void ThrowIfCannotBeCancel()
        {
            if (Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно отменить заказ", this);
            }
        }

        public void Cancel(string cancelReason, DateTime now)
        {
            ThrowIfCannotBeCancel();

            CanceledAt = now;
            CancelReason = cancelReason;
            QueueCategory = QueueCategory.Unspecified;
            ProcessingStream = null;
            Status = ReviewOrderStatus.Canceled;
            InProgressAt = null;
        }

        public long GetTotalAmount()
        {
            long paymentAmount = GetPaymentAmount(Transactions);
            long servicePaymentsAmount = GetPaymentAmount(DetailedReviewPayment?.Transactions);

            long result = Type switch
            {
                ReviewOrderType.OutOfQueue => 0,
                ReviewOrderType.Donation => paymentAmount + servicePaymentsAmount,
                ReviewOrderType.Free => Price + paymentAmount + servicePaymentsAmount,
                ReviewOrderType.Charity => 0,
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new InvalidOperationException("Неподдерживаемый тип заказа"),
            };

            return result;
        }

        public long GetPaidPriorityAmount()
        {
            return Type switch
            {
                ReviewOrderType.Donation or ReviewOrderType.Free => GetPaymentAmount(Transactions),
                ReviewOrderType.OutOfQueue or ReviewOrderType.Charity => 0,
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new InvalidOperationException("Неподдерживаемый тип заказа"),
            };
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

        private void RecalculatePaymentState(long requiredAmount)
        {
            long paymentAmount = GetPaymentAmount(Transactions);
            long payableAmount = requiredAmount > paymentAmount
                ? requiredAmount - paymentAmount
                : 0;

            if (TrackUrl is null)
            {
                Status = ReviewOrderStatus.Preorder;
                PayableAmount = payableAmount;

                return;
            }

            Status = payableAmount > 0
                ? ReviewOrderStatus.AwaitingPayment
                : ReviewOrderStatus.Pending;

            PayableAmount = payableAmount;
        }
    }
}
