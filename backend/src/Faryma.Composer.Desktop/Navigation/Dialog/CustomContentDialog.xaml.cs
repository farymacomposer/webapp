using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.Navigation
{
    public sealed partial class CustomContentDialog : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CustomContentDialog),
            null);

        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register(
            nameof(DialogContent),
            typeof(object),
            typeof(CustomContentDialog),
            null);

        public static readonly DependencyProperty CloseButtonCommandProperty = DependencyProperty.Register(
            nameof(CloseButtonCommand),
            typeof(ICommand),
            typeof(CustomContentDialog),
            null);

        public static readonly DependencyProperty IsCloseButtonVisibleProperty = DependencyProperty.Register(
            nameof(IsCloseButtonVisible),
            typeof(bool),
            typeof(CustomContentDialog),
            new PropertyMetadata(true));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object DialogContent
        {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public ICommand CloseButtonCommand
        {
            get => (ICommand)GetValue(CloseButtonCommandProperty);
            set => SetValue(CloseButtonCommandProperty, value);
        }

        public bool IsCloseButtonVisible
        {
            get => (bool)GetValue(IsCloseButtonVisibleProperty);
            set => SetValue(IsCloseButtonVisibleProperty, value);
        }

        public CustomContentDialog()
        {
            InitializeComponent();
        }
    }
}
