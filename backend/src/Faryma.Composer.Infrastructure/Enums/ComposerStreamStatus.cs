using System.ComponentModel;

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
        [Description("Запланирован")]
        Planned = 1,

        /// <summary>
        /// Идет в данный момент (стрим запущен)
        /// </summary>
        [Description("⚫ Live")]
        Live = 2,

        /// <summary>
        /// Завершен
        /// </summary>
        [Description("Завершен")]
        Completed = 3,

        /// <summary>
        /// Отменен
        /// </summary>
        [Description("Отменен")]
        Canceled = 4
    }
}