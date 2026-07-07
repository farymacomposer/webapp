namespace Faryma.Composer.Domain.Entities.TransactionSources
{
    /// <summary>
    /// Оплата подробного разбора заказа
    /// </summary>
    public sealed class ReviewOrderDetailedReviewPaymentEntity : TransactionSourceEntity
    {
        public long ReviewOrderId { get; set; }

        /// <summary>
        /// Стоимость подробного разбора
        /// </summary>
        public required long Price { get; set; }

        // Навигационные свойства

        /// <summary>
        /// Заказ разбора, для которого оплачена услуга подробного разбора
        /// </summary>
        public required ReviewOrderEntity ReviewOrder { get; set; }

        /// <summary>
        /// Погашение жетона, дающее право на подробный разбор
        /// </summary>
        public UserEntitlementRedemptionEntity? CoverageRedemption { get; set; }
    }
}
