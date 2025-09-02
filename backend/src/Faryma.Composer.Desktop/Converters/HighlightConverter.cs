using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Faryma.Composer.Desktop.Converters
{
    public sealed partial class HighlightConverter : IValueConverter
    {
        const double _saturation = 80;
        const double _lightness = 60;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int hue = Math.Abs(GetSimpleDeterministicHash(value.ToString()!)) % 360;
            Color color = ColorHelper.FromHsl(hue, _saturation, _lightness);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;

        private static int GetSimpleDeterministicHash(string s)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in s)
                {
                    hash = (hash * 31) + c;
                }

                return hash / 10;
            }
        }
    }
}