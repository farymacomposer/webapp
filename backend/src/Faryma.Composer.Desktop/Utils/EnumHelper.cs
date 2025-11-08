using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Faryma.Composer.Desktop.Utils
{
    public static class EnumHelper
    {
        public static string? GetDescription(object value) => value
            ?.GetType()
            .GetField(value.ToString()!)
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description;

        public static IEnumerable<(int Value, string? Description)> GetValues<TEnum>() where TEnum : struct, Enum => Enum
            .GetValues<TEnum>()
            .Select(x => (Unsafe.As<TEnum, int>(ref x), GetDescription(x)));
    }
}