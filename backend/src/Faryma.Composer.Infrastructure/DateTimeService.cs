namespace Faryma.Composer.Infrastructure
{
    public sealed class DateTimeService
    {
        public DateTime Now { get; }
        public DateOnly Today { get; }

        public DateTimeService()
        {
            Now = DateTime.UtcNow;
            Today = DateOnly.FromDateTime(Now);
        }
    }
}