using System.Globalization;

namespace Faryma.Composer.Desktop.Utils
{
    public static class DateOnlyExtensions
    {
        private static readonly CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");

        /// <summary>
        /// Возвращает первый день месяца
        /// </summary>
        public static DateOnly GetFirstDayOfMonth(this DateOnly date) => new(date.Year, date.Month, 1);

        /// <summary>
        /// Возвращает последний день месяца
        /// </summary>
        public static DateOnly GetLastDayOfMonth(this DateOnly date) => new(date.Year, date.Month, date.GetDaysInMonth());

        /// <summary>
        /// Возвращает количество дней в месяце
        /// </summary>
        public static int GetDaysInMonth(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue).GetDaysInMonth();

        /// <summary>
        /// Возвращает название месяца ("январь", "февраль", ...)
        /// </summary>
        public static string GetMonthName(this DateOnly date) => _ruCulture.DateTimeFormat.GetMonthName(date.Month);

        /// <summary>
        /// Возвращает день начала недели
        /// </summary>
        public static DateOnly StartOfWeek(this DateOnly date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;

            return date.AddDays(-1 * diff);
        }

        /// <summary>
        /// Возвращает сокращенное название дня недели ("пн", "вт", ...)
        /// </summary>
        public static string GetAbbreviatedDayName(this DateOnly date) => _ruCulture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);

        /// <summary>
        /// Возвращает название дня недели ("понедельник", "вторник", ...)
        /// </summary>
        public static string GetDayName(this DateOnly date) => _ruCulture.DateTimeFormat.GetDayName(date.DayOfWeek);
    }
}
