namespace Faryma.Composer.Api.Test.Infrastructure
{
    public static class DateTimeTestAssertions
    {
        public static void AssertSameInstant(DateTime? expected, DateTime? actual)
        {
            if (expected is null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(TruncateToMilliseconds(expected.Value), TruncateToMilliseconds(actual.Value));
        }

        public static DateTime TruncateToMilliseconds(DateTime value) =>
            new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Utc);
    }
}
