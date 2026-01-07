using Faryma.Composer.Infrastructure.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class ComposerStreamStatusHighlightConverter : IValueConverter
    {
        private static readonly Dictionary<ComposerStreamStatus, SolidColorBrush> _brushes = new()
        {
            [ComposerStreamStatus.Planned] = new SolidColorBrush(Colors.RoyalBlue),
            [ComposerStreamStatus.Live] = new SolidColorBrush(Colors.Goldenrod),
            [ComposerStreamStatus.Completed] = new SolidColorBrush(Colors.SeaGreen),
            [ComposerStreamStatus.Canceled] = new SolidColorBrush(Colors.Gray),
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ComposerStreamStatus enumValue && _brushes.TryGetValue(enumValue, out SolidColorBrush? brush))
            {
                return brush;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}