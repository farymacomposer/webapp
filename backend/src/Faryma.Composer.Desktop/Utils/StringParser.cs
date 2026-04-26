using System.Globalization;
using System.Numerics;

namespace Faryma.Composer.Desktop.Utils
{
    public static class StringParser
    {
        private static readonly CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");

        private static readonly (NumberStyles style, CultureInfo provider)[] _formats =
        [
            (NumberStyles.Float, _ruCulture),
            (NumberStyles.Float, CultureInfo.InvariantCulture),
            (NumberStyles.Number, _ruCulture),
        ];

        public static bool TryParseNumber<T>(object? value, out T? result) where T : INumber<T>
        {
            if (value is null)
            {
                result = T.Zero;
                return false;
            }

            if (value is T t)
            {
                result = t;
                return true;
            }

            if (value is string s)
            {
                return TryParseNumber(s, out result);
            }

            return TryParseNumber(value.ToString(), out result);
        }

        public static bool TryParseNumber<T>(string? value, out T? result) where T : INumber<T>
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = T.Zero;
                return false;
            }

            foreach ((NumberStyles style, CultureInfo provider) in _formats)
            {
                if (T.TryParse(value, style, provider, out result))
                {
                    return true;
                }
            }

            result = T.Zero;
            return false;
        }

        public static bool TryParseTimeSpan(string? value, out TimeSpan result)
        {
            value = value?.Trim();

            if (int.TryParse(value, out int timeValue))
            {
                timeValue = Math.Abs(timeValue);
                decimal hoursWithFraction = timeValue / 100m;
                var hours = (int)Math.Truncate(hoursWithFraction);
                var minutes = (int)(hoursWithFraction % 1 * 100);
                if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59)
                {
                    result = new TimeSpan(hours, minutes, 0);
                    return true;
                }
            }
            else if (TimeSpan.TryParse(value, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryParseDateTime(string? value, out DateTime result)
        {
            value = value?.Trim();

            foreach (CultureInfo provider in new[]
            {
                _ruCulture,
                CultureInfo.InvariantCulture,
            })
            {
                if (DateTime.TryParse(value, provider, out result))
                {
                    return true;
                }
            }

            result = DateTime.MinValue;
            return false;
        }
    }
}
