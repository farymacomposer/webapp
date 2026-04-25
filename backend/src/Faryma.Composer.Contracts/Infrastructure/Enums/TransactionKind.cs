namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Тип транзакции
    /// </summary>
    public enum TransactionKind
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Пополнение счета
        /// </summary>
        AccountTopUp = 1,

        /// <summary>
        /// Оплата заказа/услуги
        /// </summary>
        Payment = 2,

        /// <summary>
        /// Отмена транзакции
        /// </summary>
        Reversal = 3,
    }
}
