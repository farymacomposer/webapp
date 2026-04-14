namespace Faryma.Composer.Infrastructure
{
    public sealed class DateTimeService(DateTime now)
    {
        public DateTime Now { get; } = now;
        public DateOnly Today { get; } = DateOnly.FromDateTime(now);

        public DateTimeService() : this(DateTime.UtcNow)
        {
        }
    }
}