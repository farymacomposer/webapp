namespace Faryma.Composer.Core.Utils
{
    public static class DateOnlyExtensions
    {
        public static DateOnly GetNextDateForDay(this DateOnly dateTime, DayOfWeek targetDayOfWeek)
        {
            int daysUntilTarget = ((int)targetDayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;

            return dateTime.AddDays(daysUntilTarget);
        }
    }
}