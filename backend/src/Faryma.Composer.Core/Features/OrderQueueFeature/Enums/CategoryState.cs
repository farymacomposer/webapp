namespace Faryma.Composer.Core.Features.OrderQueueFeature.Enums
{
    /// <summary>
    /// Состояния категорий заказов, определяющие текущий приоритет обработки
    /// </summary>
    public enum CategoryState
    {
        /// <summary>
        /// Состояние не определено
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Начальное состояние
        /// </summary>
        Initial = 1,

        /// <summary>
        /// Категория - вне очереди
        /// </summary>
        OutOfQueue = 2,

        /// <summary>
        /// Донатная категория
        /// </summary>
        Donation = 3,

        /// <summary>
        /// Долговые категории
        /// </summary>
        Debt = 4,

        /// <summary>
        /// Обработка заказов завершена
        /// </summary>
        Completed = 5,
    }
}