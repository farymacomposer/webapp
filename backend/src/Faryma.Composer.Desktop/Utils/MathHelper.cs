namespace Faryma.Composer.Desktop.Utils
{
    public static class MathHelper
    {
        public static double? Round(double? value, int digits) => value.HasValue ? Math.Round(value.Value, digits, MidpointRounding.AwayFromZero) : null;
        public static int? Round(double? value) => value.HasValue ? (int)Math.Round(value.Value, MidpointRounding.AwayFromZero) : null;
    }
}