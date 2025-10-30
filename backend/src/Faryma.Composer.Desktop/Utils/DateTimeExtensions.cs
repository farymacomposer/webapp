using Microsoft.VisualBasic;

namespace Faryma.Composer.Desktop.Utils
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Округляет указанное значение DateTime вниз до ближайшего интервала .Trim(TimeSpan.TicksPerMinute)
        /// </summary>
        public static DateTime Trim(this DateTime date, long roundTicks) => new(date.Ticks - (date.Ticks % roundTicks), date.Kind);

        /// <summary>
        /// Возвращает количество дней в месяце
        /// </summary>
        public static int GetDaysInMonth(this DateTime date) => DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Возвращает первый день месяца
        /// </summary>
        public static DateTime GetFirstDayOfMonth(this DateTime date) => new(date.Year, date.Month, 1);

        /// <summary>
        /// Возвращает последний день месяца
        /// </summary>
        public static DateTime GetLastDayOfMonth(this DateTime date) => new(date.Year, date.Month, date.GetDaysInMonth());

        /// <summary>
        /// Возвращает последнюю миллисекунду дня
        /// </summary>
        public static DateTime GetEndOfDay(this DateTime date) => date.Date.AddDays(1).AddMilliseconds(-1);

        /// <summary>
        /// Возвращает день недели
        /// </summary>
        public static int GetWeekday(this DateTime date) => DateAndTime.Weekday(date, FirstDayOfWeek.Monday);

        /// <summary>
        /// Возвращает название дня недели
        /// </summary>
        public static string GetWeekdayName(this DateTime date, bool abbreviate = false)
        {
            int weekday = date.GetWeekday();

            return DateAndTime.WeekdayName(weekday, abbreviate, FirstDayOfWeek.Monday);
        }

        /// <summary>
        /// Возвращает день начала недели
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;

            return date.AddDays(-1 * diff).Date;
        }
    }
}