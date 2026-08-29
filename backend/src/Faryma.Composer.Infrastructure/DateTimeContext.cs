namespace Faryma.Composer.Infrastructure
{
    public sealed class DateTimeContext(DateTime now)
    {
        public DateTime Now { get; } = now;
        public DateOnly Today { get; } = DateOnly.FromDateTime(now);

        public DateTimeContext() : this(DateTime.UtcNow)
        {
        }
    }
}
