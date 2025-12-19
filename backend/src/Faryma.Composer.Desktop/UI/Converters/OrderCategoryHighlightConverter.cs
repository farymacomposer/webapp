using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class OrderCategoryHighlightConverter : IValueConverter
    {
        private static readonly Dictionary<OrderCategoryType, SolidColorBrush> _brushes = new()
        {
            [OrderCategoryType.Unspecified] = new SolidColorBrush(Colors.Gray),
            [OrderCategoryType.OutOfQueue] = new SolidColorBrush(Colors.Gold),
            [OrderCategoryType.Donation] = new SolidColorBrush(Colors.YellowGreen),
            [OrderCategoryType.Debt] = new SolidColorBrush(Colors.SlateBlue),
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderCategoryType enumValue && _brushes.TryGetValue(enumValue, out SolidColorBrush? brush))
            {
                return brush;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}