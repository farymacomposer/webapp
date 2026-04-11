using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class QueueCategoryHighlightConverter : IValueConverter
    {
        private static readonly Dictionary<QueueCategory, SolidColorBrush> _brushes = new()
        {
            [QueueCategory.Unspecified] = new SolidColorBrush(Colors.Gray),
            [QueueCategory.OutOfQueue] = new SolidColorBrush(Colors.Gold),
            [QueueCategory.Donation] = new SolidColorBrush(Colors.YellowGreen),
            [QueueCategory.Debt] = new SolidColorBrush(Colors.SlateBlue),
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is QueueCategory enumValue && _brushes.TryGetValue(enumValue, out SolidColorBrush? brush))
            {
                return brush;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}