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
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Запланирован
        /// </summary>
        [Description("Запланирован")]
        Planned = 1,

        /// <summary>
        /// Идет в данный момент (стрим запущен)
        /// </summary>
        [Description("Запущен")]
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