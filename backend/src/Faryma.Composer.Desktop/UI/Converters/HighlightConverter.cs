using Faryma.Composer.Infrastructure.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ColorHelper = Faryma.Composer.Desktop.Utils.ColorHelper;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class HighlightConverter : IValueConverter
    {
        private const double _saturation = 80;
        private const double _lightness = 60;

        private static readonly Dictionary<OrderCategoryType, Color> _categoryTypeColors = new()
        {
            [OrderCategoryType.Unspecified] = Colors.Gray,
            [OrderCategoryType.OutOfQueue] = Colors.Gold,
            [OrderCategoryType.Donation] = Colors.YellowGreen,
            [OrderCategoryType.Debt] = Colors.SlateBlue,
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                int hue = Math.Abs(GetSimpleDeterministicHash(str)) % 360;
                Color color = ColorHelper.FromHsl(hue, _saturation, _lightness);

                return new SolidColorBrush(color);
            }

            if (value is OrderCategoryType categoryType)
            {
                return new SolidColorBrush(_categoryTypeColors[categoryType]);
            }

            return DependencyProperty.UnsetValue;
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