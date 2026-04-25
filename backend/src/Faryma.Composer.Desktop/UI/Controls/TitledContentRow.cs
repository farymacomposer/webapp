using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class TitledContentRow : ContentControl
    {
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            nameof(Spacing),
            typeof(double),
            typeof(TitledContentRow),
            new PropertyMetadata(16.0));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(TitledContentRow),
            new PropertyMetadata(null));

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public TitledContentRow()
        {
            DefaultStyleKey = typeof(TitledContentRow);
        }
    }
}
