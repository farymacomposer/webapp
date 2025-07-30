namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Тип заказа разбора трека
    /// </summary>
    public enum ReviewOrderType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Вне очереди
        /// </summary>
        OutOfQueue = 1,

        /// <summary>
        /// Донат
        /// </summary>
        Donation = 2,

        /// <summary>
        /// Бесплатный
        /// </summary>
        Free = 3
    }
}