using Faryma.Composer.Desktop.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class StringHashHighlightConverter : IValueConverter
    {
        private const double _saturation = 80;
        private const double _lightness = 60;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                int hue = Math.Abs(GetSimpleDeterministicHash(str)) % 360;
                Color color = ColorHelper.FromHsl(hue, _saturation, _lightness);

                return new SolidColorBrush(color);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;

        private static int GetSimpleDeterministicHash(string text)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in text)
                {
                    hash = (hash * 31) + c;
                }

                return hash / 10;
            }
        }
    }
}