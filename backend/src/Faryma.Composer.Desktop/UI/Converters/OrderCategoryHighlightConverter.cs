using Faryma.Composer.Infrastructure.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Faryma.Composer.Desktop.UI
{
    /// <summary>
    /// Конвертер для подсветки по категории заказа
    /// </summary>
    public sealed partial class OrderCategoryHighlightConverter : IValueConverter
    {
        private static readonly Dictionary<OrderCategoryType, SolidColorBrush> _categoryTypeBrushes = new()
        {
            [OrderCategoryType.Unspecified] = new SolidColorBrush(Colors.Gray),
            [OrderCategoryType.OutOfQueue] = new SolidColorBrush(Colors.Gold),
            [OrderCategoryType.Donation] = new SolidColorBrush(Colors.YellowGreen),
            [OrderCategoryType.Debt] = new SolidColorBrush(Colors.SlateBlue),
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderCategoryType categoryType && _categoryTypeBrushes.TryGetValue(categoryType, out SolidColorBrush? brush))
            {
                return brush;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}