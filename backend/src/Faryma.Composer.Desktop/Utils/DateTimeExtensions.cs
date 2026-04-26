using System.Globalization;

namespace Faryma.Composer.Desktop.Utils
{
    public static class DateTimeExtensions
    {
        private static readonly CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");

        /// <summary>
        /// Возвращает первый день месяца
        /// </summary>
        public static DateTime GetFirstDayOfMonth(this DateTime date) => new(date.Year, date.Month, 1);

        /// <summary>
        /// Возвращает последний день месяца
        /// </summary>
        public static DateTime GetLastDayOfMonth(this DateTime date) => new(date.Year, date.Month, date.GetDaysInMonth());

        /// <summary>
        /// Возвращает количество дней в месяце
        /// </summary>
        public static int GetDaysInMonth(this DateTime date) => DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Возвращает название месяца ("январь", "февраль", ...)
        /// </summary>
        public static string GetMonthName(this DateTime date) => _ruCulture.DateTimeFormat.GetMonthName(date.Month);

        /// <summary>
        /// Возвращает день начала недели
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;

            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Возвращает сокращенное название дня недели ("пн", "вт", ...)
        /// </summary>
        public static string GetAbbreviatedDayName(this DateTime date) => _ruCulture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);

        /// <summary>
        /// Возвращает название дня недели ("понедельник", "вторник", ...)
        /// </summary>
        public static string GetDayName(this DateTime date) => _ruCulture.DateTimeFormat.GetDayName(date.DayOfWeek);

        /// <summary>
        /// Округляет указанное значение DateTime вниз до ближайшего интервала .Trim(TimeSpan.TicksPerMinute)
        /// </summary>
        public static DateTime Trim(this DateTime date, long roundTicks) => new(date.Ticks - (date.Ticks % roundTicks), date.Kind);

        /// <summary>
        /// Возвращает последнюю миллисекунду дня
        /// </summary>
        public static DateTime GetEndOfDay(this DateTime date) => date.Date.AddDays(1).AddMilliseconds(-1);
    }
}
