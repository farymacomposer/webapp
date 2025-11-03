using Faryma.Composer.Desktop.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class EnumDescriptionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language) => EnumHelper.GetDescription(value);
        public object? ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}