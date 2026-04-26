using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace Faryma.Composer.Desktop.UI
{
    public sealed class BindingProxy : DependencyObject
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ObservableObject),
            typeof(BindingProxy),
            new PropertyMetadata(null));

        public ObservableObject ViewModel
        {
            get => (ObservableObject)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}
