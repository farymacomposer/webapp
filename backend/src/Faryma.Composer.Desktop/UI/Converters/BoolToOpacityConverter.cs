using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                return boolean ? 1.0 : 0.4;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}