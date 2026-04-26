using Faryma.Composer.Desktop.Utils;

namespace Faryma.Composer.Desktop.Validation
{
    public static class PropertyValidator
    {
        public static bool SetFloat(ref string? field, string? newValue, ValidationRange range, string? defaultValue = null, string format = "F2")
        {
            if (field == newValue)
            {
                return false;
            }

            if (!StringParser.TryParseNumber(newValue, out float value))
            {
                field = defaultValue;

                return true;
            }

            if (value < range.Min)
            {
                value = range.Min;
            }

            if (value > range.Max)
            {
                value = range.Max;
            }

            field = value.ToString(format);

            return true;
        }

        public static bool SetInt(ref string? field, string? newValue, ValidationRange range, string? defaultValue = null)
        {
            if (field == newValue)
            {
                return false;
            }

            if (!StringParser.TryParseNumber(newValue, out int value))
            {
                field = defaultValue;

                return true;
            }

            if (value < range.Min)
            {
                value = (int)range.Min;
            }

            if (value > range.Max)
            {
                value = (int)range.Max;
            }

            field = value.ToString();

            return true;
        }
    }
}
