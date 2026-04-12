namespace Faryma.Composer.Infrastructure
{
    public sealed class DateTimeService
    {
        public DateTime Now { get; }
        public DateOnly Today { get; }

        public DateTimeService() : this(DateTime.UtcNow)
        {
        }

        public DateTimeService(DateTime now)
        {
            Now = now;
            Today = DateOnly.FromDateTime(now);
        }
    }
}