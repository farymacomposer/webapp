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
        /// Донат
        /// </summary>
        Donation = 1,

        /// <summary>
        /// Вне очереди
        /// </summary>
        OutOfQueue = 2,

        /// <summary>
        /// Бесплатный
        /// </summary>
        Free = 3
    }
}