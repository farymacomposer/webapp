namespace Faryma.Composer.Infrastructure.Enums
{
    /// <summary>
    /// Статус стрима
    /// </summary>
    public enum ComposerStreamStatus
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Запланирован
        /// </summary>
        Planned = 1,

        /// <summary>
        /// Идет в данный момент (стрим запущен)
        /// </summary>
        Live = 2,

        /// <summary>
        /// Завершен
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Отменен
        /// </summary>
        Canceled = 4
    }
}