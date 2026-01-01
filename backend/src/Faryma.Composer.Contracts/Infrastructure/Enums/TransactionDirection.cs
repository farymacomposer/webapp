namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Направление транзакции (увеличение или уменьшение баланса)
    /// </summary>
    public enum TransactionDirection
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Зачисление (увеличивает баланс)
        /// </summary>
        Credit = 1,

        /// <summary>
        /// Списание (уменьшает баланс)
        /// </summary>
        Debit = 2
    }
}