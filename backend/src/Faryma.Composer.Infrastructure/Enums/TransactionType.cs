namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Тип операции
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Пополнение
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// Оплата
        /// </summary>
        Payment = 2
    }
}