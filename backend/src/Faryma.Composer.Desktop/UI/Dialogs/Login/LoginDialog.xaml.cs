using Faryma.Composer.Desktop.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Faryma.Composer.Desktop.UI
{
    public sealed partial class LoginDialog : UserControl, IDialogControl<LoginDialogVM>
    {
        public LoginDialogVM ViewModel { get; } = App.GetService<LoginDialogVM>();

        public LoginDialog()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs _)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }
    }
}